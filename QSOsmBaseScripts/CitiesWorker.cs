using System;
using Npgsql;
using System.IO;
using System.Linq;

namespace OsmBaseScripts
{
	public struct City
	{
		public int Id;
		public string Name;
		public string Place;
		public string SuburbDistrict;
		public string[] Ways;

		public City (string name, string place, string[] ways, string suburbDistrict = null)
		{
			Id = 0;
			Name = name;
			Place = place;
			Ways = ways;
			SuburbDistrict = suburbDistrict;
		}

		public City (int id, string name, string[] ways)
		{
			Id = id;
			Name = name;
			Place = String.Empty;
			Ways = ways;
			SuburbDistrict = String.Empty;
		}
	}

	public static class CitiesWorker
	{
		public static void FillCities ()
		{
			FileInfo file = new FileInfo ("./SqlScripts/create_city_tables.sql");
			string script = file.OpenText ().ReadToEnd ();
			Console.WriteLine ("Создаем таблицы городов...");
			using (var connection = Connection.GetPostgisConnection ()) {
				connection.Open ();
				using (var cmd = new NpgsqlCommand (script, connection)) {
					cmd.ExecuteReader ();
				}
			}
			Console.WriteLine ("\tOK");

			FillSuburbDistrictsTable ();
			FillCityDistrictsTable ();
			FillCitiesTable ();
		}

		static void FillCitiesTable ()
		{
			string selectQuery = "SELECT name, place, tags->'addr:suburbdistrict', way::text " +
			                     "FROM planet_osm_polygon " +
			                     "WHERE place IN ('city', 'town', 'village', 'hamlet', 'farm', 'allotments', 'isolated_dwelling') " +
			                     "AND name IS NOT NULL " +
			                     "AND name <> '' " +
								 "ORDER BY name, tags->'addr:suburbdistrict';";
			string insertQuery = "INSERT " +
			                     "INTO osm_cities (id, name, place, suburb_district_id)" +
			                     "VALUES (@id, @name, @place, " +
			                     "(SELECT id FROM osm_suburb_districts WHERE name = @suburb_district LIMIT 1));";
			string insertWithoutDistrictQuery = "INSERT " +
			                                    "INTO osm_cities (id, name, place, suburb_district_id)" +
			                                    "VALUES (@id, @name, @place, NULL);";
			string insertWayQuery = "INSERT " +
				"INTO osm_city_ways (city_id, way)" +
				"VALUES (@city_id, @way);";
			
			Console.WriteLine ("Заполняем города...");
			using (var connection = Connection.GetPostgisConnection ()) {
				connection.Open ();
				using (var cmdSelect = new NpgsqlCommand (selectQuery, connection)) {
					using (var reader = cmdSelect.ExecuteReader ()) {
						using (var innerConnection = Connection.GetPostgisConnection ()) {
							innerConnection.Open ();
							int id = 0;
							string lastName = null, lastDistrict = null;
							while (reader.Read ()) {
								var district = reader.IsDBNull(2) ? null : reader.GetString(2);
								if(lastName != reader.GetString (0) || lastDistrict != district)
								{
									id++;
									using (var cmdInsert = new NpgsqlCommand ((reader.IsDBNull (2) ? insertWithoutDistrictQuery : insertQuery), innerConnection)) {
										cmdInsert.Parameters.AddWithValue ("@id", id);
										cmdInsert.Parameters.AddWithValue ("@name", reader.GetString (0));
										cmdInsert.Parameters.AddWithValue ("@place", reader.GetString (1));
										if (!reader.IsDBNull (2))
											cmdInsert.Parameters.AddWithValue ("@suburb_district", reader.GetString (2));
										cmdInsert.ExecuteNonQuery ();
										Console.Write ("\r\tВсего добавлено городов {0}... ", id);
										lastName = reader.GetString(0);
										lastDistrict = district;
									}
								}
								//insert way
								using (var cmdInsert = new NpgsqlCommand (insertWayQuery, innerConnection)) {
									cmdInsert.Parameters.AddWithValue ("@city_id", id);
									cmdInsert.Parameters.AddWithValue ("@way", reader.GetString (3));
									cmdInsert.ExecuteNonQuery ();
								}
							}
						}
					}
				}
			}
			Console.Write ("OK\n");
		}

		static void FillCityDistrictsTable ()
		{
			string selectQuery = "SELECT name, way::text " +
			                     "FROM planet_osm_polygon " +
			                     "WHERE admin_level = '5' " +
			                     "AND boundary = 'administrative' " +
			                     "AND name IS NOT NULL AND name <> '';";
			string insertQuery = "INSERT " +
			                     "INTO osm_city_districts (id, name, way) " +
			                     "VALUES (@id, @name, @way);";

			Console.WriteLine ("Заполняем районы города...");
			FillDistrict (selectQuery, insertQuery);
		}

		static void FillSuburbDistrictsTable ()
		{
			string selectQuery = "SELECT name, way::text " +
			                     "FROM planet_osm_polygon " +
			                     "WHERE admin_level = '6' " +
			                     "AND boundary = 'administrative' " +
			                     "AND name IS NOT NULL AND name <> '';";
			string insertQuery = "INSERT " +
			                     "INTO osm_suburb_districts (id, name, way) " +
			                     "VALUES (@id, @name, @way);";

			Console.WriteLine ("Заполняем районы области...");
			FillDistrict (selectQuery, insertQuery);
		}

		static void FillDistrict (string selectQuery, string insertQuery)
		{
			using (var connection = Connection.GetPostgisConnection ()) {
				connection.Open ();
				using (var cmdSelect = new NpgsqlCommand (selectQuery, connection)) {
					using (var reader = cmdSelect.ExecuteReader ()) {
						using (var innerConnection = Connection.GetPostgisConnection ()) {
							innerConnection.Open ();
							int id = 1;
							while (reader.Read ()) {
								using (var cmdInsert = new NpgsqlCommand (insertQuery, innerConnection)) {
									cmdInsert.Parameters.AddWithValue ("@id", id);
									cmdInsert.Parameters.AddWithValue ("@name", reader.GetString (0));
									cmdInsert.Parameters.AddWithValue ("@way", reader.GetString (1));
									cmdInsert.ExecuteNonQuery ();
									Console.Write ("\r\tВсего обработано {0} записей... ", id);
									id++;
								}
							}
						}
					}
				}
			}
			Console.Write ("OK\n");
		}
	}
}

