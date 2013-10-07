using System;
using MySql.Data.MySqlClient;

namespace QSProjectsLib
{
	public class DBWorks
	{
		public DBWorks()
		{
		}

		public static int GetInt(MySqlDataReader rdr, string Column, int NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetInt32(Column);
			else
				return NullValue;
		}

		public static long GetLong(MySqlDataReader rdr, string Column, long NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetInt64(Column);
			else
				return NullValue;
		}

		public static decimal GetDecimal(MySqlDataReader rdr, string Column, decimal NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetDecimal(Column);
			else
				return NullValue;
		}

		public static float GetFloat(MySqlDataReader rdr, string Column, float NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetFloat(Column);
			else
				return NullValue;
		}

		public static double GetDouble(MySqlDataReader rdr, string Column, double NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetDouble(Column);
			else
				return NullValue;
		}

		public static DateTime GetDateTime(MySqlDataReader rdr, string Column, DateTime NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetDateTime(Column);
			else
				return NullValue;
		}

		public static string GetString(MySqlDataReader rdr, string Column, string NullValue)
		{
			if (rdr[Column] != DBNull.Value)
				return rdr.GetString(Column);
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

	}
}

