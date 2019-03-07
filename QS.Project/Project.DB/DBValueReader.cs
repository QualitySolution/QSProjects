using System;
using System.Data;
namespace QS.Project.DB
{
	public class DBValueReader
	{
		private readonly IDataReader reader;

		public DBValueReader(IDataReader reader)
		{
			this.reader = reader;
		}

		#region OnlyValues
		public bool GetBoolean(string Column, bool NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetBoolean(reader.GetOrdinal(Column));
			else
				return NullValue;
		}

		public int GetInt(string Column, int NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetInt32(reader.GetOrdinal(Column));
			else
				return NullValue;
		}

		public long GetLong(string Column, long NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetInt64(reader.GetOrdinal(Column));
			else
				return NullValue;
		}

		public decimal GetDecimal(int column, decimal NullValue)
		{
			if(reader[column] != DBNull.Value)
				return reader.GetDecimal(column);
			else
				return NullValue;
		}


		public decimal GetDecimal(string Column, decimal NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetDecimal(reader.GetOrdinal(Column));
			else
				return NullValue;
		}

		public float GetFloat(string Column, float NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetFloat(reader.GetOrdinal(Column));
			else
				return NullValue;
		}

		public double GetDouble(string Column, double NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetDouble(reader.GetOrdinal(Column));
			else
				return NullValue;
		}

		public DateTime GetDateTime(string Column, DateTime NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetDateTime(reader.GetOrdinal(Column));
			else
				return NullValue;
		}

		public string GetString(string Column, string NullValue)
		{
			if(reader[Column] != DBNull.Value)
				return reader.GetString(reader.GetOrdinal(Column));
			else
				return NullValue;
		}
		#endregion

		#region NullableValues
		public bool? GetBoolean(string Column)
		{
			return reader[Column] != DBNull.Value ? (bool?)reader.GetBoolean(reader.GetOrdinal(Column)) : null;
		}

		public int? GetInt(string Column)
		{
			return reader[Column] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal(Column)) : null;
		}

		public long? GetLong(string Column)
		{
			return reader[Column] != DBNull.Value ? (long?)reader.GetInt64(reader.GetOrdinal(Column)) : null;
		}

		public decimal? GetDecimal(string Column)
		{
			return reader[Column] != DBNull.Value ? (decimal?)reader.GetDecimal(reader.GetOrdinal(Column)) : null;
		}

		public float? GetFloat(string Column)
		{
			return reader[Column] != DBNull.Value ? (float?)reader.GetFloat(reader.GetOrdinal(Column)) : null;
		}

		public double? GetDouble(string Column)
		{
			return reader[Column] != DBNull.Value ? (double?)reader.GetDouble(reader.GetOrdinal(Column)) : null;
		}

		public DateTime? GetDateTime(string Column)
		{
			return reader[Column] != DBNull.Value ? (DateTime?)reader.GetDateTime(reader.GetOrdinal(Column)) : null;
		}

		public string GetString(string Column)
		{
			return reader[Column] != DBNull.Value ? reader.GetString(reader.GetOrdinal(Column)) : null;
		}
		#endregion
	}
}
