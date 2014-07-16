using System;
using System.Data.Common;

namespace QSProjectsLib
{
	public class DBWorks
	{
		public DBWorks()
		{
		}

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

		public class SQLHelper
		{
			public string Text;

			private bool FirstInList = true;
			private string ListSeparator = ", ";
			private string BeforeFirst = "";
			public QuoteType QuoteMode = QuoteType.None;

			public SQLHelper(string text)
			{
				Text = text;
			}
				
			public void AddAsList(string item, string separator)
			{
				ListSeparator = separator;
				AddAsList (item);
			}

			public void AddAsList(string item)
			{
				if(FirstInList && BeforeFirst != "")
					Text += BeforeFirst;
				if (!FirstInList)
					Text += ListSeparator;
				if (QuoteMode == QuoteType.SingleQuotes)
					Text += String.Format("'{0}'", item);
				else if (QuoteMode == QuoteType.DoubleQuotes)
					Text += String.Format("\"{0}\"", item);
				else
					Text += item;
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
				Text += text;
			}
		}

		public enum QuoteType {None, SingleQuotes, DoubleQuotes};
	}
}

