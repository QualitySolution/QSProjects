using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DbManagement
{
	public class MariaDBConnectionInfo : ConnectionInfo {
		public override IDbProvider CreateProvider() => new MariaDBProvider(this);
	}
}
