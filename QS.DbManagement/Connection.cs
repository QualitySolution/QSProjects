using System;
using System.Collections.Generic;

namespace QS.DbManagement
{
    public class Connection : ICloneable
    {
		public ConnectionInfo ConnectionInfo { get; set; }

		public string ConnectionTitle { get; set; }

		public string User { get; set; }

		public bool Last { get; set; } = false;

		public object Clone() => new Connection {
			ConnectionInfo = (ConnectionInfo)ConnectionInfo.Clone(),
			ConnectionTitle = ConnectionTitle,
			User = User
		};

		public Dictionary<string, string> PrepareParams() {
			var dict = new Dictionary<string, string> {
				{ nameof(ConnectionInfo.Title), ConnectionInfo.Title },
				{ nameof(ConnectionTitle), ConnectionTitle },
				{ nameof(User), User }
			};
			if(Last)
				dict.Add(nameof(Last), "true");

			foreach(var parameter in ConnectionInfo.Parameters)
				dict.Add(parameter.Title, parameter.Value as string);

			return dict;
		}
	}
}
