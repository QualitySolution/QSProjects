using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using NLog;

namespace QSCustomFields
{
	public partial class CFFieldPropertys : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private bool newItem = true;
		public string TableName;
		string OriginalFieldName;
		int OriginalSize;

		public CFFieldPropertys ()
		{
			this.Build ();
		}

		public void Fill(int id)
		{
			newItem = false;
			logger.Info("Запрос поля №{0}...", id);
			string sql = String.Format ("SELECT {0}.* FROM {0} " +
			                            "WHERE {0}.id = @id", CFMain.FieldsInfoTable);
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", id);

				using (MySqlDataReader rdr = cmd.ExecuteReader())
				{
					if(!rdr.Read())
						return;

					entryID.Text = rdr.GetString ("id");
					entryName.Text = rdr.GetString ("name");
					//checkbuttonDisplay.Active = rdr.GetBoolean ("display");
					//checkbuttonSearch.Active = rdr.GetBoolean ("search");
					entryDBName.Text = rdr.GetString ("columnname");
					OriginalFieldName = rdr.GetString ("columnname");
					TableName = rdr.GetString ("tablename");
				}

				this.Title = entryName.Text;

				System.Data.DataTable schema = QSMain.connectionDB.GetSchema("Columns", new string[4] { null, 
					QSMain.ConnectionDB.Database,
					TableName,
					entryDBName.Text});

				//Заполняем тип
				switch (schema.Rows[0]["DATA_TYPE"].ToString ()) {
				case "varchar":
					comboType.Active = 0;
					spinSize.Value = Convert.ToDouble (schema.Rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
					OriginalSize = Convert.ToInt32 (schema.Rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
					break;
				default:
					throw new ApplicationException(String.Format ("Тип поля {0} не поддерживается программой.", schema.Rows[0]["DATA_TYPE"]));
				}
				//Запрещаем менять тип поля
				comboType.Sensitive = false;

				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				logger.ErrorException("Ошибка получения информации о поле!", ex);
				QSMain.ErrorMessage(this,ex);
			}

			TestCanSave();
		}

		void TestCanSave()
		{
			bool Nameok = entryName.Text != "";
			bool DBNameOk = entryDBName.Text == "" ||
				System.Text.RegularExpressions.Regex.IsMatch (entryDBName.Text, "^[a-zA-Z0-9_]+$");
			bool TypeOk = comboType.Active >= 0;

			buttonOk.Sensitive = Nameok && DBNameOk && TypeOk;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction ();
			logger.Info("Записываем информацию о поле...");
			try
			{
				// Работаем с внутренними данными
				string sql;
				if(newItem)
					sql = String.Format ("INSERT INTO {0}(name, columnname, tablename)" +
					                     "VALUES (@name, @columnname, @tablename)", CFMain.FieldsInfoTable);
				else
					sql = String.Format ("UPDATE {0} SET name = @name, columnname = @columnname, tablename = @tablename WHERE id = @id",
					                     CFMain.FieldsInfoTable);
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", entryID.Text);
				cmd.Parameters.AddWithValue ("@name", entryName.Text);
				cmd.Parameters.AddWithValue ("@columnname", entryDBName.Text);
				cmd.Parameters.AddWithValue ("@tablename", TableName);
				//cmd.Parameters.AddWithValue ("@display", checkbuttonDisplay.Active);
				//cmd.Parameters.AddWithValue ("@search", checkbuttonSearch.Active);
				cmd.ExecuteNonQuery ();

				//Работаем с таблицей БД
				if(newItem)
				{ 
					CreateField (trans);
				}
				else if(OriginalFieldName != entryDBName.Text || OriginalSize != spinSize.ValueAsInt)
				{
					ChangeField (trans);
				}

				trans.Commit ();
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				trans.Rollback ();
				logger.ErrorException("Ошибка сохранения поля!", ex);
				QSMain.ErrorMessage(this,ex);
			}
		}

		void CreateField(MySqlTransaction trans)
		{
			string sql = String.Format ("ALTER TABLE {0} ADD COLUMN {1} VARCHAR({2}) NULL DEFAULT NULL", TableName, entryDBName.Text, spinSize.Text);
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
			cmd.ExecuteNonQuery ();
		}

		void ChangeField(MySqlTransaction trans)
		{
			string sql = "ALTER TABLE " + TableName + 
				" CHANGE COLUMN " + OriginalFieldName + " " + entryDBName.Text +
			                            " VARCHAR(" + spinSize.Text +") NULL DEFAULT NULL";
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
			cmd.ExecuteNonQuery ();
		}

	}
}

