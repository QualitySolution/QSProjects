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
		int OriginalSize, OriginalDigits;

		public CFFieldPropertys ()
		{
			this.Build ();
			OnComboDataTypeChanged (comboDataType, EventArgs.Empty);
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
					comboFieldType.Active = (int)Enum.Parse (typeof(FieldTypes), rdr.GetString("type"), true);
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
					comboDataType.Active = (int)FieldDataTypes.varchar;
					spinSize.Value = Convert.ToDouble (schema.Rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
					OriginalSize = Convert.ToInt32 (schema.Rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
					break;
				case "decimal":
					comboDataType.Active = (int)FieldDataTypes.DECIMAL;
					spinSize.Value = Convert.ToDouble (schema.Rows[0]["NUMERIC_PRECISION"]);
					OriginalSize = Convert.ToInt32 (schema.Rows[0]["NUMERIC_PRECISION"]);
					spinDigits.Value = Convert.ToDouble (schema.Rows[0]["NUMERIC_SCALE"]);
					OriginalDigits = Convert.ToInt32 (schema.Rows[0]["NUMERIC_SCALE"]);
					break;
				default:
					throw new ApplicationException(String.Format ("Тип поля {0} не поддерживается программой.", schema.Rows[0]["DATA_TYPE"]));
				}
				//Запрещаем менять тип поля
				comboFieldType.Sensitive = false;

				TestCanSave ();
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
			bool DBNameOk = entryDBName.Text != "" && entryDBName.Text.IndexOfAny (new char[]{ ' ', '.' }) < 0;
			bool dataTypeOk = comboDataType.Active >= 0;
			bool fieldTypeOk = comboFieldType.Active >= 0;
			bool sizeOk = comboDataType.Active != (int)FieldDataTypes.DECIMAL || spinSize.ValueAsInt > spinDigits.ValueAsInt;

			buttonOk.Sensitive = Nameok && DBNameOk && dataTypeOk && fieldTypeOk && sizeOk;
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
					sql = String.Format ("INSERT INTO {0}(name, columnname, tablename, type)" +
					                     "VALUES (@name, @columnname, @tablename, @type)", CFMain.FieldsInfoTable);
				else
					sql = String.Format ("UPDATE {0} SET name = @name, columnname = @columnname, tablename = @tablename, type = @type WHERE id = @id",
					                     CFMain.FieldsInfoTable);
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", entryID.Text);
				cmd.Parameters.AddWithValue ("@name", entryName.Text);
				cmd.Parameters.AddWithValue ("@columnname", entryDBName.Text);
				cmd.Parameters.AddWithValue ("@tablename", TableName);
				cmd.Parameters.AddWithValue ("@type", ((FieldTypes)comboFieldType.Active).ToString ());
				//cmd.Parameters.AddWithValue ("@display", checkbuttonDisplay.Active);
				//cmd.Parameters.AddWithValue ("@search", checkbuttonSearch.Active);
				cmd.ExecuteNonQuery ();

				//Работаем с таблицей БД
				if(newItem)
				{ 
					CreateField (trans);
				}
				else if(OriginalFieldName != entryDBName.Text || OriginalSize != spinSize.ValueAsInt || 
				        (comboDataType.Active == (int)FieldDataTypes.DECIMAL && OriginalDigits != spinDigits.ValueAsInt))
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
			string sql;
			switch ((FieldDataTypes) comboDataType.Active) {
			case FieldDataTypes.varchar :
				sql = String.Format ("ALTER TABLE {0} ADD COLUMN {1} VARCHAR({2}) NULL DEFAULT NULL", TableName, entryDBName.Text, spinSize.Text);
				break;
			case FieldDataTypes.DECIMAL :
				sql = String.Format ("ALTER TABLE {0} ADD COLUMN {1} DECIMAL({2},{3}) NULL DEFAULT NULL", TableName, entryDBName.Text, spinSize.Text, spinDigits.Text);
				break;
			default:
				throw new ApplicationException(String.Format ("Не поддеживается создание колонки типа {0}", ((FieldDataTypes)comboDataType.Active).ToString ()));
			}
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
			cmd.ExecuteNonQuery ();
		}

		void ChangeField(MySqlTransaction trans)
		{
			string sql;
			switch ((FieldDataTypes)comboDataType.Active) {
			case FieldDataTypes.varchar:
				sql = String.Format("ALTER TABLE {0} CHANGE COLUMN {1} {2} VARCHAR({3}) NULL DEFAULT NULL", TableName, OriginalFieldName, entryDBName.Text, spinSize.Text);
				break;
			case FieldDataTypes.DECIMAL:
				sql = String.Format("ALTER TABLE {0} CHANGE COLUMN {1} {2} DECIMAL({3},{4}) NULL DEFAULT NULL", TableName, OriginalFieldName, entryDBName.Text, spinSize.Text, spinDigits.Text);
				break;
			default:
				throw new ApplicationException(String.Format ("Не поддеживается изменение колонки для типа {0}", ((FieldDataTypes)comboDataType.Active).ToString ()));
			}
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
			cmd.ExecuteNonQuery ();
		}

		protected void OnEntryNameChanged(object sender, EventArgs e)
		{
			TestCanSave ();
		}

		protected void OnComboFieldTypeChanged(object sender, EventArgs e)
		{
			switch (comboFieldType.Active) {
			case 0:
				comboDataType.Active = (int)FieldDataTypes.varchar;
				break;
			case 1:
				comboDataType.Active = (int)FieldDataTypes.DECIMAL;
				break;
			default:
				logger.Warn ("Неизвестный тип поля.");
				break;
			}
			TestCanSave ();
		}

		protected void OnEntryDBNameChanged(object sender, EventArgs e)
		{
			TestCanSave ();
		}

		protected void OnComboDataTypeChanged(object sender, EventArgs e)
		{
			switch (comboDataType.Active) {
			case 0:
				labelSizeSeparator.Visible = spinDigits.Visible = false;
				spinSize.Adjustment.ClampPage (0, 255);
				spinSize.TooltipText = "Количество символов";
				break;
			case 1:
				labelSizeSeparator.Visible = spinDigits.Visible = true;
				spinSize.TooltipText = "Количество значащих знаков";
				spinSize.Adjustment.ClampPage (1, 65);
				break;
			default:
				logger.Warn ("Неизвестный тип данных.");
				break;
			}
			TestCanSave ();
		}

		protected void OnSpinSizeChanged(object sender, EventArgs e)
		{
			TestCanSave ();
		}

		protected void OnSpinDigitsChanged(object sender, EventArgs e)
		{
			TestCanSave ();
		}
	}
}

