using MySqlConnector;
using QS.DBScripts.Controllers;
using System;
using System.Collections.Generic;

namespace QS.DbManagement.Creation {
	public class DbCreationFactory
	{
		private readonly DbResourcesCreationMap _map;

		public DbCreationFactory(DbResourcesCreationMap map) {
			_map = map;
		}

		public IDbCreatorModel Create<Arg>(Arg resources) where Arg : DbCreationResources
		{
			return (IDbCreatorModel)_map.Resolve(resources);
		}
	}
}
