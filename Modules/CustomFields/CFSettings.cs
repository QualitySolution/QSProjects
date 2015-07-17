using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using NLog;

namespace QSCustomFields
{
	public partial class CFSettings : Gtk.Window
	{ 
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ListStore TablesStore, FieldsStore;
		private CFTable CurrentTable;

		enum TablesCol{
			dbName,
			title,
			count
		}
		enum FieldsCol{
			id,
			name,
			dbname
		}


		public CFSettings() : 
		base(Gtk.WindowType.Toplevel)
		{
			this.Build();

			TablesStore = new ListStore(typeof(string), typeof(string), typeof(int));

			treeviewObjects.AppendColumn("Объект", new CellRendererText(), "text", (int)TablesCol.title);
			treeviewObjects.AppendColumn("Кол-во", new CellRendererText(), "text", (int)TablesCol.count);

			treeviewObjects.Model = TablesStore;
			UpdateTablesInfo ();

			//Создаем таблицу "Полей"
			FieldsStore = new Gtk.ListStore (typeof (int), //0 - ID
			                                     typeof (string), // 1 - Name
			                                     typeof (string) // 2 - DB_name
			                                    );
			treeviewFields.AppendColumn ("Имя", new Gtk.CellRendererText (), "text", (int)FieldsCol.name );
			treeviewFields.AppendColumn ("Имя в БД", new Gtk.CellRendererText (), "text", (int)FieldsCol.dbname);

			treeviewFields.Model = FieldsStore;
			treeviewFields.ShowAll();
		}

		protected void OnTreeviewObjectsCursorChanged(object sender, EventArgs e)
		{
			bool selected = treeviewObjects.Selection.CountSelectedRows () == 1;
			if (selected) {
				TreeIter iter;
				treeviewObjects.Selection.GetSelected (out iter);
				string selectedTable = (string)TablesStore.GetValue (iter, (int)TablesCol.dbName);
				CurrentTable = CFMain.Tables.Find (t => t.DBName == selectedTable);
				labelName.LabelProp = CurrentTable.Title;
				UpdateFields ();
			} else 
			{
				CurrentTable = null;
				FieldsStore.Clear ();
			}

			buttonAdd.Sensitive = selected;
		}

		void UpdateFields()
		{
			//Получаем таблицу полей
			string sql = String.Format ("SELECT {0}.* FROM {0} " +
			                            "WHERE {0}.tablename = @table", CFMain.FieldsInfoTable);
			logger.Info ("Обновляем таблицу полей");
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)QSMain.ConnectionDB);
				cmd.Parameters.AddWithValue("@table", CurrentTable.DBName);
				using (MySqlDataReader rdr = cmd.ExecuteReader ()) 
				{
					FieldsStore.Clear ();
					while (rdr.Read ()) {
						FieldsStore.AppendValues (rdr.GetInt32 ("id"),
						                        rdr.GetString ("name"),
						                          rdr.GetString ("columnname"));
					}
				}
				logger.Info ("Ок");
			}catch (Exception ex)
			{
				logger.Error(ex, "Ошибка чтения полей {0}!", CurrentTable.DBName);
				QSMain.ErrorMessage(this,ex);
			}
			OnTreeviewFieldsCursorChanged (treeviewFields, EventArgs.Empty);
		}

		protected void OnTreeviewFieldsCursorChanged (object sender, EventArgs e)
		{
			bool SelectOk = treeviewFields.Selection.CountSelectedRows() == 1;
			buttonEdit.Sensitive = SelectOk;
			buttonDelete.Sensitive = SelectOk;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			CFFieldPropertys win = new CFFieldPropertys();
			win.TableName = CurrentTable.DBName;
			win.Show ();
			if ((Gtk.ResponseType)win.Run () == Gtk.ResponseType.Ok)
				TablesChanged ();
			win.Destroy ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			treeviewFields.Selection.GetSelected(out iter);
			int ItemId = (int)FieldsStore.GetValue(iter, (int)FieldsCol.id);

			CFFieldPropertys win = new CFFieldPropertys();
			win.TableName = CurrentTable.DBName;
			win.Fill (ItemId);
			win.Show ();
			if ((Gtk.ResponseType)win.Run () == Gtk.ResponseType.Ok)
				TablesChanged ();
			win.Destroy ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			MessageDialog md = new MessageDialog ( this, DialogFlags.Modal,
			                                      MessageType.Question, 
			                                      ButtonsType.YesNo,
			                                      String.Format ("Удалить поле из таблицы {0}? Будут потеряны все внесенные в эту колонку данные.", CurrentTable.DBName));
			ResponseType result = (ResponseType)md.Run ();
			md.Destroy ();
			if(result == ResponseType.No)
				return;

			TreeIter iter;
			treeviewFields.Selection.GetSelected(out iter);
			string FieldName = FieldsStore.GetValue(iter, (int)FieldsCol.dbname).ToString ();
			int FieldId = (int)FieldsStore.GetValue(iter, (int)FieldsCol.id);

			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction ();
			logger.Info("Удаляем поле...");
			try
			{
				//Работаем с таблицей БД
				string sql = String.Format ("ALTER TABLE {0} DROP COLUMN {1}", CurrentTable.DBName, FieldName);
				MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)QSMain.ConnectionDB, trans);
				cmd.ExecuteNonQuery ();

				// Работаем с внутренними данными
				sql = String.Format ("DELETE FROM {0} WHERE id = @id", CFMain.FieldsInfoTable);
				cmd = new MySqlCommand(sql, (MySqlConnection)QSMain.ConnectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", FieldId);
				cmd.ExecuteNonQuery ();

				trans.Commit ();
				TablesChanged ();
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				trans.Rollback ();
				logger.Error(ex, "Ошибка удаления поля!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnTreeviewFieldsRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		private void TablesChanged()
		{
			CFMain.LoadTablesFields ();
			UpdateTablesInfo ();
			UpdateFields ();
		}

		private void UpdateTablesInfo()
		{
			TablesStore.Clear ();
			foreach(CFTable table in CFMain.Tables)
			{
				TablesStore.AppendValues(table.DBName,
				                         table.Title,
				                         table.Fields.Count
				                        );
			}
		}
	}
}

