using System;
using Npgsql;

namespace OsmBaseScripts
{
	public static class Connection
	{
		public static NpgsqlConnection GetPostgisConnection ()
		{
			return new NpgsqlConnection ("Host=localhost;Username=postgres;Password=postgres;Database=osmgis;CommandTimeout=60");
		}
	}
}

