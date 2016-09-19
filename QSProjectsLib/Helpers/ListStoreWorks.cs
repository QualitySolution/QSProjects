using System;
using System.Data.Common;
using Gtk;
using NLog;
using System.Collections.Generic;

namespace QSProjectsLib
{
	public static class ListStoreWorks
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static bool SearchListStore( ListStore list, int id, out TreeIter iter)
		{
			return SearchListStore (list, id, 1, out iter);
		}

		public static bool SearchListStore( ListStore list, object searchvalue, int column, out TreeIter iter)
		{   // Перебираем список, ищем id, возвращаем iter
			if(list.GetIterFirst(out iter))
			{
				if( searchvalue.Equals (list.GetValue(iter, column)))
					return true;
			}
			else
				return false;
			while (list.IterNext(ref iter)) 
			{
				if( searchvalue.Equals (list.GetValue(iter, column)))
					return true;
			}
			return false;		
		}

		public static bool SearchListStore<TObject>(ListStore list, Func<TObject, bool> searchFunc, int column, out TreeIter iter)
		{   
			if(!list.GetIterFirst(out iter))
				return false;
			do {
				object item = list.GetValue (iter, column);

				if(item is TObject && searchFunc((TObject)item))
					return true;
			} while (list.IterNext (ref iter));
			return false;
		}

		public static bool SearchListStore( ListStore list, string text, out TreeIter iter)
		{   // Перебираем список, ищем Строку, возвращаем iter
			if(list.GetIterFirst(out iter))
			{
				if( list.GetValue(iter,0).ToString() == text)
					return true;
			}
			else
				return false;
			while (list.IterNext(ref iter)) 
			{
				if(list.GetValue(iter,0).ToString() == text)
					return true;
			}
			return false;		
		}

		/// <summary>Заполняет комбобокс уникальными значениями поля
		/// </summary>
		public static ListStore CreateWithUniqueValue(string tablename, string fieldname)
		{
			logger.Info("Запрос всех значений {0}.{1} ...", tablename, fieldname);
			ListStore list = new ListStore (typeof(string));
			try
			{
				string sql = "SELECT DISTINCT " + fieldname + " FROM " + tablename;
				QSMain.CheckConnectionAlive();
				DbCommand cmd = QSMain.ConnectionDB.CreateCommand();
				cmd.CommandText = sql;
				using(DbDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						if(rdr[0] != DBNull.Value)
						{
							list.AppendValues(rdr.GetString(0));
						}
					}
				}
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Ошибка получения списка уникальных значений!");
			}
			return list;
		}


		/// <summary>
		/// Метод создает новый ListStore из коллекции.
		/// </summary>
		public static ListStore CreateFromEnumerable<TNode>(IEnumerable<TNode> list)
		{
			var store = new ListStore(typeof(TNode));
			foreach (var item in list)
				store.AppendValues(item);
			return store;
		}
	}
}

