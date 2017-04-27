using System;
using Npgsql;
using NLog;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace OsmBaseScripts
{
	public class StreetsWorker
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		static List<string> WrongStreetsForMerge = new List<string> {
			"Лиговский проспект",
			"Боровая улица",
			"Дунайский проспект",
			"Воронежская улица"
		};

		struct DistrictKeyValue
		{
			public string Name;
			public string Way;

			public DistrictKeyValue (string name, string way)
			{
				Name = name;
				Way = way;
			}
		}

		struct LinkStruct
		{
			public string StreetName;
			public List<int> LinkedDistricts;

			public LinkStruct (string name, int district)
			{
				StreetName = name;
				LinkedDistricts = new List<int> ();
				LinkedDistricts.Add (district);
			}
		}

		public static void FillStreets ()
		{
			FileInfo file = new FileInfo ("./SqlScripts/create_street_tables.sql");
			string script = file.OpenText ().ReadToEnd ();
			Console.WriteLine ("Создаем таблицы улиц...");
			using (var connection = Connection.GetPostgisConnection ()) {
				connection.Open ();
				using (var cmd = new NpgsqlCommand (script, connection)) {
					cmd.ExecuteReader ();
				}
			}
			Console.WriteLine ("\tOK");


			Console.WriteLine ("Заполняем временную таблицу с улицами...");
			var proc = Process.Start ("perl", "./PerlScripts/fill_own_streets.pl");
			proc.WaitForExit ();
			Console.WriteLine ("\tOK");

			LinkStreetsToDistricts ();
		}

		static void LinkStreetsToDistricts ()
		{
			string spbWay;
			List<string> streets;
			List<string> districts;
			List<LinkStruct> links = new List<LinkStruct> ();

			string getStreetLinesQuery = "SELECT way::text " +
			                             "FROM planet_osm_line " +
			                             "WHERE name = @street " +
			                             "AND ST_Contains(@city_way, way);";

			Console.WriteLine ("Объединяем длинные улицы Санкт-Петербурга...");

			using (var connection = Connection.GetPostgisConnection ()) {
				spbWay = getSpbWay (connection);
				streets = getDuplicatingStreets (connection);
				districts = getCityDistricts (connection);	

				//Main linking cycle
				for (int i = 0; i < streets.Count; i++) {
					string currentStreet = streets [i];
					List<DistrictKeyValue> specificStreetDistricts = getSpecificStreetDistricts (connection, currentStreet);
					//Generating query
					string checkIntersectQuery = generateCheckIntersectionQuery (specificStreetDistricts);
					int districtsCount = specificStreetDistricts.Count;

					//Getting lines for it
					using (var getStreetLinesCmd = new NpgsqlCommand (getStreetLinesQuery, connection)) {
						getStreetLinesCmd.Parameters.AddWithValue ("@street", currentStreet);
						getStreetLinesCmd.Parameters.AddWithValue ("@city_way", spbWay);

						//Quering intersections for each line
						using (var reader = getStreetLinesCmd.ExecuteReader ()) {
							using (var innerConnection = Connection.GetPostgisConnection ()) {
								innerConnection.Open ();
								while (reader.Read ()) {
									using (var checkIntersectCmd = new NpgsqlCommand (checkIntersectQuery, innerConnection)) {
										checkIntersectCmd.Parameters.AddWithValue ("@way", reader.GetString (0));

										for (int j = 0; j < districtsCount; j++) {
											checkIntersectCmd.Parameters.AddWithValue ("@way" + j, specificStreetDistricts [j].Way);
										}
										using (var resultReader = checkIntersectCmd.ExecuteReader ()) {
											while (resultReader.Read ()) {
												int firstIntersection = -1;
												for (int j = 0; j < districtsCount; j++) {
													//Parsing result
													if (resultReader.GetBoolean (j)) {
														int realId = districts.FindIndex (dis => dis == specificStreetDistricts [j].Name);
														if (firstIntersection == -1) {
															firstIntersection = realId;
															//Already have street linked to this district
															if (links.Any (ls => ls.StreetName == currentStreet && ls.LinkedDistricts.Contains (realId)))
																continue;
															//Else create record
															links.Add (new LinkStruct (currentStreet, realId));
														} else {
															if (links.Any (ls => ls.StreetName == currentStreet && ls.LinkedDistricts.Contains (realId))) {
																int first = links.FindIndex (ls => ls.StreetName == currentStreet && ls.LinkedDistricts.Contains (realId));
																int second = links.FindIndex (ls => ls.StreetName == currentStreet && ls.LinkedDistricts.Contains (firstIntersection));
																if (first != second)
																	mergeTwoElements (first, second, links);
																continue;
															}
															links.Find (ls => ls.StreetName == currentStreet && ls.LinkedDistricts.Contains (firstIntersection))
																.LinkedDistricts.Add (realId);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
					Console.Write ("\r\tВсего обработано {0} записей... ", i);
				}
			}
			//Исправление ошибочных данных (улицы не объединяются корректно)
			foreach (var str in WrongStreetsForMerge) {
				int first = links.FindIndex (ln => ln.StreetName == str);
				int second = links.FindLastIndex (ln => ln.StreetName == str);
				mergeTwoElements (first, second, links);
			}

			Console.WriteLine ("\tOK");

			fillSpbStreets (links, districts);
			fillOtherStreets ();
		}

		static void fillSpbStreets (List<LinkStruct> links, List<string> districts) //TODO FIXME
		{
			int cityId = -1;
			string selectCityQuery = "SELECT id FROM osm_cities WHERE name = 'Санкт-Петербург';";
			string insertQuery = "INSERT " +
			                     "INTO osm_streets (id, name, city_id)" +
			                     "VALUES (@id, @name, @city_id);";
			string linkStreetQuery = "INSERT " +
			                         "INTO osm_streets_to_districts (street_id, district_id) " +
			                         "VALUES (@id, (SELECT id FROM osm_city_districts WHERE name = @name LIMIT 1));";

			Console.WriteLine ("Заполняем дублирующиеся улицы Санкт-Петербурга...");

			using (var connection = Connection.GetPostgisConnection ()) {
				connection.Open ();
				using (var selectCityCmd = new NpgsqlCommand (selectCityQuery, connection)) {
					using (var cityReader = selectCityCmd.ExecuteReader ()) {
						if (cityReader.Read ())
							cityId = cityReader.GetInt32 (0);
					}
				}
				if (cityId < 0) {
					logger.Error ("Не удалось получить id города в FillSpbStreets()");
					return;
				}
				int id = 1;
				foreach (var street in links) {
					using (var insertStreetCmd = new NpgsqlCommand (insertQuery, connection)) {
						insertStreetCmd.Parameters.AddWithValue ("@id", id);
						insertStreetCmd.Parameters.AddWithValue ("@name", street.StreetName);
						insertStreetCmd.Parameters.AddWithValue ("@city_id", cityId);
						insertStreetCmd.ExecuteNonQuery ();
					}
					foreach (var districtId in street.LinkedDistricts) {
						using (var linkStreetCmd = new NpgsqlCommand (linkStreetQuery, connection)) {
							linkStreetCmd.Parameters.AddWithValue ("@id", id);
							linkStreetCmd.Parameters.AddWithValue ("@name", districts [districtId]);
							linkStreetCmd.ExecuteNonQuery ();
						}
					}
					Console.Write ("\r\tВсего обработано {0} записей... ", id);
					id++;
				}
				Console.Write ("OK\n");
				Console.WriteLine ("Заполняем остальные улицы Санкт-Петербурга...");
				//TODO FIXME Неправильная выборка. Из-за нее не добавляются некоторые улицы.
				string selectStreetsQuery = "SELECT name, district " +
				                            "FROM temp_osm_streets " +
				                            "WHERE city = 'Санкт-Петербург' AND name NOT IN " +
				                            "(SELECT name FROM temp_osm_streets WHERE city = 'Санкт-Петербург' GROUP BY name HAVING (COUNT(name) > 1));";
				using (var getStreetsCmd = new NpgsqlCommand (selectStreetsQuery, connection)) {
					using (var reader = getStreetsCmd.ExecuteReader ()) {
						using (var internalConnection = Connection.GetPostgisConnection ()) {
							internalConnection.Open ();
							while (reader.Read ()) {
								string street = reader.GetString (0);
								string district = reader.GetString (1);
								using (var insertStreetCmd = new NpgsqlCommand (insertQuery, internalConnection)) {
									insertStreetCmd.Parameters.AddWithValue ("@id", id);
									insertStreetCmd.Parameters.AddWithValue ("@name", street);
									insertStreetCmd.Parameters.AddWithValue ("@city_id", cityId);
									insertStreetCmd.ExecuteNonQuery ();
								}
								using (var linkStreetCmd = new NpgsqlCommand (linkStreetQuery, internalConnection)) {
									linkStreetCmd.Parameters.AddWithValue ("@id", id);
									linkStreetCmd.Parameters.AddWithValue ("@name", district);
									linkStreetCmd.ExecuteNonQuery ();
								}
								Console.Write ("\r\tВсего обработано {0} записей... ", id);
								id++;
							}
						}
					}
				}
				Console.Write ("OK\n");
			}
		}

		static void fillOtherStreets () //TODO FIXME
		{
			int id = -1;
			List<City> cities = new List<City> ();
			string selectIdQuery = "SELECT MAX(id) FROM osm_streets;";


			string selectCitiesQuery = "SELECT id, name " +
			                           "FROM osm_cities " +
			                           "WHERE name <> 'Санкт-Петербург' AND name <> '';";
			string selectCityStreets = "SELECT tags->'addr:street' " +
			                           "FROM planet_osm_polygon " +
			                           "WHERE tags->'addr:city' = @city " +
			                           "AND (tags->'addr:street') IS NOT NULL " +
			                           "AND tags->'addr:street' <> '' " +
									   "AND EXISTS(SELECT 1 FROM osm_city_ways WHERE city_id = @city_id AND ST_Contains (osm_city_ways.way, planet_osm_polygon.way)) " +
			                           "GROUP BY tags->'addr:district', tags->'addr:street' " +
			                           "ORDER BY tags->'addr:street';";
			string insertQuery = "INSERT " +
			                     "INTO osm_streets (id, name, city_id)" +
			                     "VALUES (@id, @name, @city_id);";
			Console.WriteLine ("Заполняем улицы других городов...");
			using (var connection = Connection.GetPostgisConnection ()) {
				connection.Open ();
				//Получаем id для вставки.
				using (var cmdGetInsertId = new NpgsqlCommand (selectIdQuery, connection)) {
					using (var insertIdReader = cmdGetInsertId.ExecuteReader ()) {
						if (insertIdReader.Read ()) {
							id = insertIdReader.GetInt32 (0);
							id++;
						}
					}
				}
				using (var cmdSelectCities = new NpgsqlCommand (selectCitiesQuery, connection)) {
					using (var citiesReader = cmdSelectCities.ExecuteReader ()) {
						while (citiesReader.Read ()) {
							int cityId = citiesReader.GetInt32 (0);
							string city = citiesReader.GetString (1);
							cities.Add (new City (cityId, city, null));
						}
					}
				}
				foreach (var city in cities) {
					using (var cmdSelectCityStreets = new NpgsqlCommand (selectCityStreets, connection)) {
						cmdSelectCityStreets.Parameters.AddWithValue ("@city", city.Name);
						cmdSelectCityStreets.Parameters.AddWithValue ("@city_id", city.Id);
						using (var streetsReader = cmdSelectCityStreets.ExecuteReader ()) {
							using (var innerConnection = Connection.GetPostgisConnection ()) {
								innerConnection.Open ();
								while (streetsReader.Read ()) {
									using (var cmdInsertStreet = new NpgsqlCommand (insertQuery, innerConnection)) {
										cmdInsertStreet.Parameters.AddWithValue ("@id", id);
										cmdInsertStreet.Parameters.AddWithValue ("@name", streetsReader.GetString (0));
										cmdInsertStreet.Parameters.AddWithValue ("@city_id", city.Id);
										cmdInsertStreet.ExecuteNonQuery ();
										Console.Write ("\r\tВсего обработано {0} записей... ", id);
										id++;
									}
								}
							}
						}
					}
				}
			}
			Console.Write ("OK\n");
		}

		static string getSpbWay (NpgsqlConnection connection)
		{
			string spbWay = String.Empty;

			string getSpbWayQuery = "SELECT way::text " +
			                        "FROM planet_osm_polygon " +
			                        "WHERE place = 'city' AND name = 'Санкт-Петербург';";
			
			if (connection.State != ConnectionState.Open)
				connection.Open ();
			
			using (var getSpbWayCmd = new NpgsqlCommand (getSpbWayQuery, connection)) {
				using (var reader = getSpbWayCmd.ExecuteReader ()) {
					if (reader.Read ()) {
						spbWay = reader.IsDBNull (0) ? String.Empty : reader.GetString (0);
					}
				}
			}
			return spbWay;
		}

		static List<DistrictKeyValue> getSpecificStreetDistricts (NpgsqlConnection connection, string street)
		{
			List<DistrictKeyValue> specificStreetDistricts = new List<DistrictKeyValue> ();

			string getStreetDistrictsQuery = "SELECT temp_osm_streets.district, planet_osm_polygon.way::text " +
			                                 "FROM temp_osm_streets " +
			                                 "LEFT JOIN planet_osm_polygon ON planet_osm_polygon.name = temp_osm_streets.district " +
			                                 "WHERE temp_osm_streets.city = 'Санкт-Петербург' " +
			                                 "AND temp_osm_streets.name = @street " +
			                                 "AND temp_osm_streets.district IS NOT NULL " +
			                                 "AND temp_osm_streets.district <> '' " +
			                                 "AND planet_osm_polygon.admin_level = '5' " +
			                                 "AND planet_osm_polygon.boundary = 'administrative';";
			
			if (connection.State != ConnectionState.Open)
				connection.Open ();

			using (var getStreetDistrictsCmd = new NpgsqlCommand (getStreetDistrictsQuery, connection)) {
				getStreetDistrictsCmd.Parameters.AddWithValue ("@street", street);
				using (var reader = getStreetDistrictsCmd.ExecuteReader ()) {
					while (reader.Read ()) {
						specificStreetDistricts.Add (new DistrictKeyValue (reader.GetString (0), reader.GetString (1)));
					}
				}
			}
			return specificStreetDistricts;
		}

		static List<string> getDuplicatingStreets (NpgsqlConnection connection)
		{
			List<string> streets = new List<string> ();

			string getDuplicatingStreetsQuery =	"SELECT name FROM temp_osm_streets " +
			                                    "WHERE city = 'Санкт-Петербург' " +
			                                    "GROUP BY name HAVING (COUNT(name) > 1);";
			
			if (connection.State != ConnectionState.Open)
				connection.Open ();
			
			using (var getDuplicatingStreetsCmd = new NpgsqlCommand (getDuplicatingStreetsQuery, connection)) {
				using (var reader = getDuplicatingStreetsCmd.ExecuteReader ()) {
					while (reader.Read ()) {
						if (!reader.IsDBNull (0))
							streets.Add (reader.GetString (0));
					}
				}
			}
			return streets;
		}

		static List<string> getCityDistricts (NpgsqlConnection connection)
		{
			List<string> districts = new List<string> ();

			string getCityDistrictsQuery = "SELECT DISTINCT(district) " +
			                               "FROM temp_osm_streets " +
			                               "WHERE city = 'Санкт-Петербург';";

			if (connection.State != ConnectionState.Open)
				connection.Open ();
			
			using (var getCityDistrictsCmd = new NpgsqlCommand (getCityDistrictsQuery, connection)) {
				using (var reader = getCityDistrictsCmd.ExecuteReader ()) {
					while (reader.Read ()) {
						if (!reader.IsDBNull (0))
							districts.Add (reader.GetString (0));
					}
				}
			}
			return districts;
		}

		static void mergeTwoElements (int first, int second, List<LinkStruct> list)
		{
			if (list [first].StreetName != list [second].StreetName)
				return;
			if (first == second)
				return;
			foreach (int num in list[second].LinkedDistricts) {
				if (list [first].LinkedDistricts.Contains (num))
					continue;
				else
					list [first].LinkedDistricts.Add (num);
			}
			list.RemoveAt (second);
		}

		static string generateCheckIntersectionQuery (List<DistrictKeyValue> specificStreetDistricts)
		{
			string checkIntersectQuery = "SELECT";
			for (int j = 0; j < specificStreetDistricts.Count; j++) {
				checkIntersectQuery += String.Format (" ST_Intersects(@way{0}, @way),", j);
			}
			checkIntersectQuery = checkIntersectQuery.TrimEnd (',');
			checkIntersectQuery += ";";
			return checkIntersectQuery;
		}
	}
}