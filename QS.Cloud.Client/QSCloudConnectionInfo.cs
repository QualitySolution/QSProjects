using QS.DbManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Cloud.Client {
	public class QSCloudConnectionInfo : ConnectionInfo {
		public override IDbProvider CreateProvider() {
			return new QSCloudProvider(this);
		}
	}
}
