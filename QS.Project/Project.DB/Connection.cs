using System;
namespace QS.Project.DB
{
	public static class Connection
	{
		internal static Func<string> GetConnectionString;
		public static string ConnectionString => GetConnectionString();
	}
}
