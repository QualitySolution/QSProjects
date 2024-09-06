using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DbManagement {

	public abstract class ConnectionInfo : ICloneable {
		public string Title { get; set; }

		public List<ConnectionParameter> Parameters { get; set; } = new List<ConnectionParameter>();

		public byte[] IconBytes { get; set; }

		public abstract IDbProvider CreateProvider();

		public abstract Connection CreateConnection(IDictionary<string, string> parameters);

		public abstract object Clone();
	}
}
