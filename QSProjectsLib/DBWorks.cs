using System;
using System.Collections.Generic;

namespace QSProjectsLib
{
	public static class DBWorks
	{
		#region OnlyValues
		public static bool GetBoolean(System.Data.IDataReader rdr, string Column, bool NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetBoolean(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}

		public static int GetInt(System.Data.IDataReader rdr, string Column, int NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetInt32(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}

		public static long GetLong(System.Data.IDataReader rdr, string Column, long NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetInt64(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}

		public static decimal GetDecimal(System.Data.IDataReader rdr, string Column, decimal NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetDecimal(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}

		public static float GetFloat(System.Data.IDataReader rdr, string Column, float NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetFloat(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}

		public static double GetDouble(System.Data.IDataReader rdr, string Column, double NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetDouble(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}

		public static DateTime GetDateTime(System.Data.IDataReader rdr, string Column, DateTime NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetDateTime(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}

		public static string GetString(System.Data.IDataReader rdr, string Column, string NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetString(rdr.GetOrdinal(Column));
			else
				return NullValue;
		}
		#endregion

		#region NullableValues
		public static bool? GetBoolean(System.Data.IDataReader rdr, string Column)
		{
			return rdr[Column] != DBNull.Value ? (bool?)rdr.GetBoolean (rdr.GetOrdinal (Column)) : null;
		}

		public static int? GetInt(System.Data.IDataReader rdr, string Column)
		{
			return rdr [Column] != DBNull.Value ? (int?)rdr.GetInt32 (rdr.GetOrdinal (Column)) : null;
		}

		public static long? GetLong(System.Data.IDataReader rdr, string Column)
		{
			return rdr [Column] != DBNull.Value ? (long?)rdr.GetInt64 (rdr.GetOrdinal (Column)) : null;
		}

		public static decimal? GetDecimal(System.Data.IDataReader rdr, string Column)
		{
			return rdr [Column] != DBNull.Value ? (decimal?)rdr.GetDecimal (rdr.GetOrdinal (Column)) : null;
		}

		public static float? GetFloat(System.Data.IDataReader rdr, string Column)
		{
			return rdr [Column] != DBNull.Value ? (float?)rdr.GetFloat (rdr.GetOrdinal (Column)) : null;
		}

		public static double? GetDouble(System.Data.IDataReader rdr, string Column)
		{
			return rdr [Column] != DBNull.Value ? (double?)rdr.GetDouble (rdr.GetOrdinal (Column)) : null;
		}

		public static DateTime? GetDateTime(System.Data.IDataReader rdr, string Column)
		{
			return rdr [Column] != DBNull.Value ? (DateTime?)rdr.GetDateTime (rdr.GetOrdinal (Column)) : null;
		}

		public static string GetString(System.Data.IDataReader rdr, string Column)
		{
			return rdr [Column] != DBNull.Value ? rdr.GetString (rdr.GetOrdinal (Column)) : null;
		}
		#endregion

		/// <summary>
		/// Return values if condition is true else DBNull.
		/// </summary>
		/// <param name="condition">If set to <c>true</c> return value.</param>
		/// <param name="NotNullValue">Not null value.</param>
		/// <typeparam name="T">Type of value.</typeparam>
		public static object ValueOrNull<T>(bool condition, T NotNullValue)
		{
			if(condition)
			{
				return (object)NotNullValue;
			}
			else
			{
				return (object)DBNull.Value;
			}
		}

		public static object IdPropertyOrNull<T>(T value) where T : class
		{
			if (value == null)
				return DBNull.Value;
			var prop = value.GetType ().GetProperty ("Id");
			if (prop == null)
				throw new ArgumentException ("Для работы метода тип {0}, должен иметь свойство Id.");

			return prop.GetValue (value, null);
		}

		public static string SQLReplaceWhere(string sql, string newWhere)
		{
			int startWhere = sql.IndexOf ("WHERE", StringComparison.OrdinalIgnoreCase);
			if (startWhere < 0)
				return sql;
			string[] stopWords = new string[] {
				"GROUP BY",
				"HAVING",
				"ORDER BY",
				"LIMIT",
				"PROCEDURE",
				"INTO OUTFILE"
			};
			int endWhere = sql.Length - 1;
			foreach(string word in stopWords)
			{
				int index = sql.IndexOf (word, startWhere, StringComparison.OrdinalIgnoreCase);
				if (index >= 0)
					endWhere = index;
			}
			sql = sql.Remove (startWhere + 5, endWhere - startWhere - 5);
			return sql.Insert (startWhere + 6, String.Format (" {0} ", newWhere.Trim ()));
		}

		public static T FineById<T>(List<T> list, object id) where T : class
		{
			if (id == null || id == DBNull.Value)
				return null;
			var prop = typeof(T).GetProperty ("Id");
			ulong idNum = Convert.ToUInt64 (id);
			if (prop == null)
				throw new ArgumentException ("Для работы метода тип {0}, должен иметь свойство Id.");
			foreach(T item in list)
			{
				if (Convert.ToUInt64(prop.GetValue (item, null)) == idNum)
					return item;
			}
			return null;
		}

		public class SQLHelper
		{
			private System.Text.StringBuilder text;

			private bool FirstInList = true;
			private string ListSeparator = ", ";
			private string BeforeFirst = "";
			public QuoteType QuoteMode = QuoteType.None;

			public SQLHelper(string text)
			{
				this.text = new System.Text.StringBuilder(text);
			}

			public SQLHelper()
			{
				this.text = new System.Text.StringBuilder();
			}

			public SQLHelper(string text, params object[] args)
			{
				this.text = new System.Text.StringBuilder(String.Format (text, args));
			}

			public string Text {
				get {
					return text.ToString ();
				}
				set {
					text = new System.Text.StringBuilder (value);
				}
			}
				
			public void AddAsList(string item, string separator)
			{
				ListSeparator = separator;
				AddAsList (item);
			}

			public void AddAsList(string item, params object[] args)
			{
				AddAsList (String.Format(item, args));
			}

			public void AddAsList(string item)
			{
				if(FirstInList && BeforeFirst != "")
					text.Append (BeforeFirst);
				if (!FirstInList)
					text.Append (ListSeparator);
				if (QuoteMode == QuoteType.SingleQuotes)
					text.AppendFormat("'{0}'", item);
				else if (QuoteMode == QuoteType.DoubleQuotes)
					text.AppendFormat("\"{0}\"", item);
				else
					text.Append (item);
				FirstInList = false;
			}

			public void StartNewList(string beforefirst = "", string separator = ", ")
			{
				FirstInList = true;
				BeforeFirst = beforefirst;
				ListSeparator = separator;
			}

			public void Add(string text)
			{
				this.text.Append (text);
			}

			public void Add(string text, params object[] args)
			{
				this.text.AppendFormat (text, args);
			}

		}

		public enum QuoteType {None, SingleQuotes, DoubleQuotes};
	}
}

