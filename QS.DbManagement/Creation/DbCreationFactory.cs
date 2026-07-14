using QS.DBScripts.Controllers;
using System;
using System.Reflection;

namespace QS.DbManagement.Creation {
	public class DbCreationFactory
	{
		private readonly DbResourcesCreationMap map;

		public DbCreationFactory(DbResourcesCreationMap map) {
			this.map = map ?? throw new ArgumentNullException(nameof(map));
		}

		public IDbCreatorModel Create(DbCreationResources resources)
		{
			try {
				return (IDbCreatorModel)map.Resolve(resources);
			}
			catch(TargetInvocationException ex) when(ex.InnerException != null) {
				throw ex.InnerException;
			}
		}
	}
}
