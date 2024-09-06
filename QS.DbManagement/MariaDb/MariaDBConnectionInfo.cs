using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DbManagement
{
	public class MariaDBConnectionInfo : ConnectionInfo {

		public string Address { get; set; }

		public MariaDBConnectionInfo(IEnumerable<ConnectionParameter> parameters) {
			Parameters = parameters.ToList();
			foreach(var parameter in parameters) {
				switch(parameter.Title) {
					case "Адрес":
						Address = (string)parameter.Value;
						break;
					default:
						throw new ArgumentException($"Неизвестный параметр {parameter.Title}");
				}
			}
		}

		public override object Clone() => new MariaDBConnectionInfo(Parameters) {
			IconBytes = (byte[])IconBytes.Clone(),
			Title = (string)Title.Clone()
		};

		public override Connection CreateConnection(IDictionary<string, string> parameters) {
			ConnectionInfo info = new MariaDBConnectionInfo(
				parameters
					.Where(pair => pair.Key != "ConnectionTitle" && pair.Key != "Title" && pair.Key != "User" && pair.Key != "last")
					.Select(pair => new ConnectionParameter(pair.Key, pair.Value))) {
				Title = "MariaDB",
				IconBytes = IconBytes
			};


			return new Connection {
				ConnectionInfo = info,
				ConnectionTitle = parameters["ConnectionTitle"],
				User = parameters["User"],
				Last = parameters.ContainsKey("Last") && parameters["Last"] == "true"
			};
		}

		public override IDbProvider CreateProvider() => new MariaDBProvider(this);
	}
}
