using System;
using System.Collections.Generic;
using NLog;
using Gtk;
using QSProjectsLib;
using MySql.Data.MySqlClient;

namespace QSCustomFields
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CustomFields : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private Dictionary<int, Gtk.Label> FieldLables;
		private Dictionary<int, Gtk.Widget> FieldWidgets;
		private CFTable usedTable;
		private int id;

		public CustomFields ()
		{
			this.Build ();
		}

		public CFTable UsedTable {
			get {
				return usedTable;
			}
			set {
				usedTable = value;
				UpdateWidgets ();
			}
		}

		public int ObjectId {
			get {
				return id;
			}
			set {
				id = value;
			}
		}

		void UpdateWidgets()
		{
			FieldLables = new Dictionary<int, Label>();
			FieldWidgets = new Dictionary<int, Gtk.Widget>();
			foreach(Widget child in tableGrid.Children)
			{
				tableGrid.Remove (child);
				child.Destroy ();
			}
			tableGrid.NRows = (uint)usedTable.Fields.Count;
			uint Row = 0;
			foreach(CFFieldInfo field in usedTable.Fields)
			{
				Label NameLable = new Label(field.Name + ":");
				NameLable.Xalign = 1;
				tableGrid.Attach(NameLable, 0, 1, Row, Row+1, 
				                             AttachOptions.Fill, AttachOptions.Fill, 0, 0);
				FieldLables.Add(field.ID, NameLable);
				Gtk.Widget ValueWidget;
				switch (field.Type) {
				case FieldTypes.varchar :
					ValueWidget = new Entry();
					((Entry)ValueWidget).MaxLength = field.Size;
					break;
				default :
					ValueWidget = new Label();
					break;
				}
				tableGrid.Attach((Widget)ValueWidget, 1, 2, Row, Row+1, 
				                 AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Fill, 0, 0);
				FieldWidgets.Add(field.ID, ValueWidget);
				Row++;
			}
			tableGrid.ShowAll();
		}

		public void LoadDataFromDB(int id)
		{
			if(usedTable.Fields.Count < 1)
			{
				logger.Info ("Нет полей для загрузки.");
				return;
			}
			logger.Info ("Загружаем данные настраиваемых полей для id={0}", id);
			this.id = id;
			DBWorks.SQLHelper sql = new DBWorks.SQLHelper ("SELECT ");
			foreach(CFFieldInfo field in usedTable.Fields)
			{
				sql.AddAsList (String.Format ("{0}.{1}", usedTable.DBName, field.ColumnName));
			};
			sql.Add (" FROM {0} WHERE {0}.id = @id ", usedTable.DBName);
			logger.Debug (sql.Text);
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql.Text, (MySqlConnection)QSMain.ConnectionDB);
				cmd.Parameters.AddWithValue("@id", id);
				using (MySqlDataReader rdr = cmd.ExecuteReader ()) 
				{
					rdr.Read ();
					foreach(CFFieldInfo field in usedTable.Fields)
					{
						switch (field.Type) {
						case FieldTypes.varchar :
							Entry wid = (Entry)FieldWidgets[field.ID];
							wid.Text = DBWorks.GetString(rdr, field.ColumnName, "");
							break;
						}
					}
				}
				logger.Info ("Ок");
			}catch (Exception ex)
			{
				string mes = "Ошибка чтения данных для настраиваемых полей!";
				logger.ErrorException(mes, ex);
				throw new ApplicationException (mes, ex);
			}
		}

		public void SaveToDB(MySqlTransaction trans)
		{
			logger.Info ("Сохраняем данные настраиваемых полей для id={0}", id);
			DBWorks.SQLHelper sql = new DBWorks.SQLHelper ("UPDATE {0} SET ", usedTable.DBName);
			foreach(CFFieldInfo field in usedTable.Fields)
			{
				sql.AddAsList (String.Format ("{0} = @{0}", field.ColumnName));
			};
			sql.Add (" WHERE id = @id ");
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql.Text, (MySqlConnection)QSMain.ConnectionDB, trans);
				cmd.Parameters.AddWithValue("@id", id);
				foreach(CFFieldInfo field in usedTable.Fields)
				{
					switch (field.Type) {
					case FieldTypes.varchar :
						Entry wid = (Entry)FieldWidgets[field.ID];
						cmd.Parameters.AddWithValue(field.ColumnName, DBWorks.ValueOrNull (wid.Text != "", wid.Text));
						break;
					}
				}
				cmd.ExecuteNonQuery ();
				logger.Info ("Записано.");
			}catch (Exception ex)
			{
				string mes = "Ошибка чтения данных для настраиваемых полей!";
				logger.ErrorException(mes, ex);
				throw new ApplicationException (mes, ex);
			}
		}

	}
}

