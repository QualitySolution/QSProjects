using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib;

namespace QSCustomFields
{
	public class CFMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public static List<CFTable> Tables;
		public static string FieldsInfoTable = "custom_fields";

		public static void LoadTablesFields()
		{
			string sql = String.Format ("SELECT {0}.* FROM {0}", CFMain.FieldsInfoTable);
			logger.Info ("Обновляем таблицу полей");
			try
			{
				foreach(CFTable table in Tables)
				{
					table.Fields.Clear ();
				}

				MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)QSMain.ConnectionDB);
				using (MySqlDataReader rdr = cmd.ExecuteReader ()) 
				{
					while (rdr.Read ()) 
					{
						CFTable workTable = Tables.Find (t => t.DBName == rdr.GetString ("tablename"));
						if(workTable == null)
						{
							workTable = new CFTable(rdr.GetString ("tablename"), rdr.GetString ("tablename"));
							Tables.Add (workTable);
						}
						CFFieldInfo field = new CFFieldInfo();
						field.ID = rdr.GetInt32 ("id");
						field.Name = rdr.GetString ("name");
						field.FieldType = (FieldTypes)Enum.Parse (typeof(FieldTypes), rdr.GetString("type"), true);
						field.ColumnName = rdr.GetString ("columnname");
						workTable.Fields.Add (field);
					}
				}

				foreach(CFTable table in Tables.FindAll (t => t.Fields.Count > 0))
				{
					foreach(CFFieldInfo field in table.Fields)
					{
						System.Data.DataTable schema = ((MySqlConnection)QSMain.ConnectionDB).GetSchema("Columns", new string[4] { null, 
							((MySqlConnection)QSMain.ConnectionDB).Database,
							table.DBName,
							field.ColumnName});

						if(schema.Rows.Count < 1)
							throw new ApplicationException(String.Format ("Колонка {0}, не найдена в таблице {1}", field.ColumnName, table.DBName));
						//Debug Только для дебага.
						foreach (System.Data.DataRow row in schema.Rows)
						{
							foreach (System.Data.DataColumn col in schema.Columns)
							{
								logger.Debug("{0} = {1}", col.ColumnName, row[col]);
							}
							logger.Debug("============================");
						}
						//Заполняем тип
						switch (schema.Rows[0]["DATA_TYPE"].ToString ()) {
						case "varchar":
							field.DataType = FieldDataTypes.varchar;
							field.Size = Convert.ToInt32 (schema.Rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
							break;
						case "decimal":
							field.DataType = FieldDataTypes.DECIMAL;
							field.Size = Convert.ToInt32 (schema.Rows[0]["NUMERIC_PRECISION"]);
							field.Digits = Convert.ToInt32 (schema.Rows[0]["NUMERIC_SCALE"]);
							break;
						default:
							throw new ApplicationException(String.Format ("Тип поля {0} не поддерживается программой.", schema.Rows[0]["DATA_TYPE"]));
						}
					}
				}
				logger.Info ("Ок");
			}catch (Exception ex)
			{
				logger.Error(ex, "Ошибка чтения полей!");
				throw ex;
			}
		}

		public static CFTable GetTableByName(string name)
		{
			return Tables.Find (t => t.DBName == name);
		}
	}

	public enum FieldDataTypes {
		varchar,
		DECIMAL
	}

	public enum FieldTypes {
		TString,
		TCurrency
	}

	public class CFTable
	{
		public string Title;
		public string DBName;
		public List<CFFieldInfo> Fields;

		public CFTable(string dbname, string title)
		{
			Title = title;
			DBName = dbname;
			Fields = new List<CFFieldInfo> ();
		}
	}

	public class CFFieldInfo
	{
		public int ID;
		public string Name;
		public string ColumnName;
		public FieldTypes FieldType;
		public FieldDataTypes DataType;
		public int Size;
		public int Digits;
	}
}

