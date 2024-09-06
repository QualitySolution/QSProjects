using QS.DbManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Cloud.Client {
	public class QSCloudConnectionInfo : ConnectionInfo {

		public string Login { get; set; }

		public QSCloudConnectionInfo(IEnumerable<ConnectionParameter> parameters) {
			Parameters = parameters.ToList();
			foreach(var parameter in parameters) {
				switch(parameter.Title) {
					case "Логин":
						Login = parameter.Value as string;
						break;
					default:
						throw new ArgumentException($"Неизвестный параметр {parameter.Title}");
				}
			}
		}

		public override IDbProvider CreateProvider() {
			return new QSCloudProvider(this);
		}

		public override Connection CreateConnection(IDictionary<string, string> parameters) {
			// TODO: deal with Icon
			ConnectionInfo info = new QSCloudConnectionInfo(
				parameters
					.Where(pair => pair.Key != "ConnectionTitle" && pair.Key != "Title" && pair.Key != "User")
					.Select(pair => new ConnectionParameter(pair.Key, pair.Value))) {
				Title = "QS Cloud",
				IconBytes = IconBytes
			};


			return new Connection {
				ConnectionInfo = info,
				ConnectionTitle = parameters["ConnectionTitle"],
				User = parameters["User"],
			};
		}

		public override object Clone() => new QSCloudConnectionInfo(Parameters.ToList()) {
			IconBytes = IconBytes,
			Title = (string)Title.Clone()
		};
	}
}
