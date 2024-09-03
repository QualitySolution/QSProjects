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
			throw new NotImplementedException();
		}

		public override object Clone() {
			return new ExampleConnectionInfo(Parameters);
		}

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
