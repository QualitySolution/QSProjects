using System;
using MySql.Data.MySqlClient;
using Gtk;

namespace QSProjectsLib
{
	public class ComboWorks
	{
		/// <summary>Заполняет комбобокс любыми данными из базы.
    	/// <param name="listmode">Режимы списка:
		/// 0 - Только элементы;
		/// 1 - Есть пункт "Все" с кодом 0;
		/// 2 - Есть пункт "Нет" с кодом -1;</param> </summary>
		public static void ComboFillUniversal(ComboBox combo, string SqlSelect, string DisplayString, MySqlParameter[] Parameters, int KeyField, int listmode, bool WithDBValues = false)
		{   
			combo.Clear ();
			CellRendererText text = new CellRendererText ();
			ListStore store;
			if(WithDBValues)
				store = new ListStore (typeof (string), typeof (int), typeof(object[]));
			else
				store = new ListStore (typeof (string), typeof (int));
			combo.Model = store;
			combo.PackStart (text, false);
			combo.AddAttribute (text, "text", 0);
			QSMain.OnNewStatusText("Запрос элементов комбобокс...");
			try
			{
				int count = 0;
				MySqlCommand cmd = new MySqlCommand(SqlSelect, QSMain.connectionDB);
				if(Parameters != null)
					cmd.Parameters.AddRange (Parameters);
				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{
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
						if(WithDBValues)
							store.AppendValues(String.Format(DisplayString, Fields),
						                   Convert.ToInt32(Fields[KeyField]),
							                   Fields);
						else
							store.AppendValues(String.Format(DisplayString, Fields),
							                   Convert.ToInt32(Fields[KeyField]));
						count++;
					}
				}
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

		/// <summary>Заполняет комбобокс элементами справочника.
		/// <param name="listmode">Режимы списка:
		/// 0 - Только элементы;
		/// 1 - Есть пункт "Все" с кодом 0;
		/// 2 - Есть пункт "Нет" с кодом -1;</param> </summary>
		public static void ComboFillReference(ComboBox combo, string TableRef, int listmode, bool SetDefault = true)
		{   			
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
				using( MySqlDataReader rdr = cmd.ExecuteReader())
				{
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
				}
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

		/// <summary>Заполняет комбобокс уникальными значениями поля
		/// </summary>
		public static void ComboFillUniqueValue(ComboBox combo, string tablename, string fieldname, bool SetDefault = true)
		{
			((ListStore)combo.Model).Clear();
			//CellRendererText text = new CellRendererText ();
			QSMain.OnNewStatusText(String.Format("Запрос всех значений {0}.{1} ...", tablename, fieldname));
			try
			{
				int count = 0;
				string sql = "SELECT DISTINCT " + fieldname + " FROM " + tablename;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						if(rdr[0] != DBNull.Value)
						{
							combo.AppendText(rdr.GetString(0));
							count++;
						}
					}
				}
				if(SetDefault && count == 1)
					combo.Active = 0;
				QSMain.OnNewStatusText("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка получения списка уникальных значений!");
			}

		}

		public static bool SetActiveItem(ComboBox combo, int id)
		{
			TreeIter iter;
			bool result = ListStoreWorks.SearchListStore((ListStore)combo.Model, id, out iter);
			if (result)
				combo.SetActiveIter(iter);
			return result;
		}

		public static bool SetActiveItem(ComboBox combo, string name)
		{
			TreeIter iter;
			bool result = ListStoreWorks.SearchListStore((ListStore)combo.Model, name, out iter);
			if (result)
				combo.SetActiveIter(iter);
			return result;
		}

		public static int GetActiveId(ComboBox combo)
		{
			TreeIter iter;
			if( combo.GetActiveIter(out iter))
				return (int) combo.Model.GetValue(iter, 1);
			else
				return -1;
		}
	}
}
