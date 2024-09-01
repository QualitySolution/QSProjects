using QS.DbManagement;
using System.Collections.Generic;
using System.Linq;

namespace QS.Cloud.Client {
	public class QSCloudConnectionInfo : ConnectionInfo {
		public override IDbProvider CreateProvider() {
			return new QSCloudProvider(this);
		}

		public override Connection CreateConnection(IDictionary<string, string> parameters) {
			// TODO: deal with Icon
			ConnectionInfo info = new QSCloudConnectionInfo {
				Title = "QS Cloud",
				Parameters = parameters
					.Where(pair => pair.Key != "ConnectionTitle" && pair.Key != "Title")
					.Select(pair => new ConnectionParameter(pair.Key, pair.Value))
					.ToList()
			};


			return new Connection {
				ConnectionInfo = info,
				ConnectionTitle = parameters["ConnectionTitle"]
			};
		}
	}
}
