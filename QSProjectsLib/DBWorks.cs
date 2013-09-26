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

