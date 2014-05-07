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

		public class SQLHelper
		{
			public string Text;

			private bool FirstInList = true;
			private string ListSeparator = ", ";
			private string BeforeFirst = "";

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
	}
}

