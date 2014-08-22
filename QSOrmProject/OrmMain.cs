using System;
using NHibernate;
using NHibernate.Cfg;

namespace QSOrmProject
{
	public static class OrmMain
	{
		public static ISessionFactory Sessions;

		public static void ConfigureOrm(string connectionString ,string[] assemblies)
		{
			var c = new Configuration(); 

			c.Configure();
			c.SetProperty("connection.connection_string", connectionString);

			foreach(string ass in assemblies)
			{
				c.AddAssembly(ass);
			}

			Sessions = c.BuildSessionFactory ();
		}
	}
}

