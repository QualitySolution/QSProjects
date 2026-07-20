using QS.DBScripts.Controllers;
using System;
using System.Reflection;

namespace QS.DbManagement.Creation {
	public class DbRewriteFactory
	{
		private readonly DbResourcesModelMap<IDbRewriteModel> map;

		public DbRewriteFactory(DbResourcesModelMap<IDbRewriteModel> map) {
			this.map = map ?? throw new ArgumentNullException(nameof(map));
		}

		public IDbRewriteModel Create(DbCreationResources resources)
		{
			try {
				return map.Resolve(resources);
			}
			catch(TargetInvocationException ex) when(ex.InnerException != null) {
				throw ex.InnerException;
			}
		}
	}
}
