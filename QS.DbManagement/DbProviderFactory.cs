using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DbManagement
{
    public class DbProviderFactory
    {
		public IDbProvider CreateDbProvider(ConnectionInfo connectionInfo) {

			switch (connectionInfo.Title) {
				case "MariaDB":
					return CreateMariaDbProvider(connectionInfo);
				default:
					throw new NotImplementedException();
			}
		}

		public IDbProvider CreateMariaDbProvider(ConnectionInfo connectionInfo)
		{
			return new MariaDbConnection(connectionInfo);
		}
    }
}
