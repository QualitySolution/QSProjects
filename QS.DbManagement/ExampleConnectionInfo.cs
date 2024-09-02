using System;
using System.Collections.Generic;

namespace QS.DbManagement {
	public class ExampleConnectionInfo : ConnectionInfo {
		public override IDbProvider CreateProvider() {
			throw new NotImplementedException("This class is only for testing");
		}

		public override Connection CreateConnection(IDictionary<string, string> parameters) {
			throw new NotImplementedException();
		}

		public ExampleConnectionInfo() {
			Parameters.Add(new ConnectionParameter("Parameter1", "Value1"));
			Parameters.Add(new ConnectionParameter("Parameter2", "Value2"));
			Parameters.Add(new ConnectionParameter("Parameter3"));

			Title = "ExampleConnection";
		}
	}
}
