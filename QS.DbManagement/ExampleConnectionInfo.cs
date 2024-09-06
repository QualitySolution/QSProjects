using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DbManagement {
	public class ExampleConnectionInfo : ConnectionInfo {

		public ExampleConnectionInfo(IEnumerable<ConnectionParameter> parameters) {
			Parameters = parameters.ToList();
		}

		public override IDbProvider CreateProvider() {
			throw new NotImplementedException("This class is only for testing");
		}

		public override Connection CreateConnection(IDictionary<string, string> parameters) {
			ConnectionInfo ci = new ExampleConnectionInfo(
				parameters
					.Where(pair => pair.Key != "User" && pair.Key != "Title" && pair.Key != "ConnectionTitle")
					.Select(pair => new ConnectionParameter(pair.Key, pair.Value))) {
				IconBytes = IconBytes,
				Title = "Example" };

			return new Connection {
				ConnectionInfo = ci,
				ConnectionTitle = parameters["ConnectionTitle"],
				User = parameters["User"]
			};
		}

		public override object Clone() => new ExampleConnectionInfo(Parameters) {
			Title = Title
		};

		public ExampleConnectionInfo() {
			Parameters = new List<ConnectionParameter> {
				new ConnectionParameter("Parameter1", "Value1"),
				new ConnectionParameter("Parameter2", "Value2"),
				new ConnectionParameter("Parameter3")
			};
			Title = "ExampleConnection";
		}
	}
}
