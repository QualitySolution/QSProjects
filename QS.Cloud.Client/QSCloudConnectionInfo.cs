using QS.DbManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Cloud.Client {
	public class QSCloudConnectionInfo : ConnectionInfo {

		public string Login { get; set; }

		public QSCloudConnectionInfo(IEnumerable<ConnectionParameter> parameters) {
			Title = "QS Cloud";

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

			var conn = new Connection();

			ConnectionInfo info = new QSCloudConnectionInfo(
				parameters
					.Where(pair => pair.Key != nameof(conn.ConnectionTitle) && pair.Key != nameof(Title)
						&& pair.Key != nameof(conn.User) && pair.Key != nameof(conn.Last))
					.Select(pair => new ConnectionParameter(pair.Key, pair.Value))) {
				IconBytes = IconBytes
			};


			return new Connection {
				ConnectionInfo = info,
				ConnectionTitle = parameters[nameof(conn.ConnectionTitle)],
				User = parameters[nameof(conn.User)],
				Last = parameters.ContainsKey(nameof(conn.Last)) && parameters[nameof(conn.Last)] == "true"
			};
		}

		public override object Clone() => new QSCloudConnectionInfo(Parameters.ToList()) {
			IconBytes = IconBytes,
			Title = (string)Title.Clone()
		};
	}
}
