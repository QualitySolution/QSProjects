using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
				}
			}
		}

		public override object Clone() => new MariaDBConnectionInfo(Parameters) {
			IconBytes = (byte[])IconBytes.Clone(),
			Title = (string)Title.Clone()
		};

		public override Connection CreateConnection(IDictionary<string, string> parameters) {
			throw new NotImplementedException();
		}

		public override IDbProvider CreateProvider() => new MariaDBProvider(this);
	}
}
