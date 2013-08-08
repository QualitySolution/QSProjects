using System;
using MySql.Data.MySqlClient;
using Gtk;

namespace QSProjectsLib
{
	public class ComboWorks
	{
		public static void ComboFillUniversal(ComboBox combo, string SqlSelect, string DisplayString, MySqlParameter[] Parameters, int KeyField, int listmode)
		{   //Заполняем универсальный комбобокс
			// Режимы списка:
			// 0 - Только элементы
			// 1 - Есть пункт "Все" с кодом 0
			// 2 - Есть пункт "Нет" с кодом -1

			combo.Clear ();
			CellRendererText text = new CellRendererText ();
			ListStore store = new ListStore (typeof (string), typeof (int));
			combo.Model = store;
			combo.PackStart (text, false);
			combo.AddAttribute (text, "text", 0);
			QSMain.OnNewStatusText("Запрос элементов комбобокс...");
			try
			{
				int count = 0;
				MySqlCommand cmd = new MySqlCommand(SqlSelect, QSMain.connectionDB);
				cmd.Parameters.AddRange (Parameters);
				MySqlDataReader rdr = cmd.ExecuteReader();

				switch (listmode) {
					case 1: //Все
					store.AppendValues("Все", 0);
					break;
					case 2: //Нет
					store.AppendValues("нет", -1);
					break;
					default:
					break;
				}

				while (rdr.Read())
				{
					object[] Fields = new object[rdr.FieldCount];
					rdr.GetValues(Fields);
					store.AppendValues(String.Format(DisplayString, Fields),
					                   Convert.ToInt32(Fields[KeyField]));
					count++;
				}
				rdr.Close();
				if(listmode == 2 && count == 1)
					combo.Active = 1;
				if(listmode == 0 && count == 1)
					combo.Active = 0;
				QSMain.OnNewStatusText("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка получения данных для комбобокс!");
			}

		}

		public static void ComboFillReference(ComboBox combo, string TableRef, int listmode, bool SetDefault = true)
		{   //Заполняем комбобокс элементами справочника
			// Режимы списка:
			// 0 - Только элементы справочника
			// 1 - Есть пункт "Все" с кодом 0
			// 2 - Есть пункт "Нет" с кодом -1
			
			combo.Clear ();
			CellRendererText text = new CellRendererText ();
			ListStore store = new ListStore (typeof (string), typeof (int));
			combo.Model = store;
			combo.PackStart (text, false);
			combo.AddAttribute (text, "text", 0);
			QSMain.OnNewStatusText("Запрос справочника " + TableRef + "...");
			try
			{
				int count = 0;
				string sql = "SELECT id, name FROM " + TableRef;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				MySqlDataReader rdr = cmd.ExecuteReader();
				
				switch (listmode) {
				case 1: //Все
					store.AppendValues("Все", 0);
					break;
				case 2: //Нет
					store.AppendValues("нет", -1);
					break;
				default:
					break;
				}
				
				while (rdr.Read())
				{
					store.AppendValues(rdr["name"].ToString(),int.Parse(rdr["id"].ToString()));
					count++;
				}
				rdr.Close();
				if(SetDefault && listmode == 2 && count == 1)
					combo.Active = 1;
				if(SetDefault && listmode == 0 && count == 1)
					combo.Active = 0;
				QSMain.OnNewStatusText("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка получения данных справочника!");
			}
			
		}
	}
}
