using System;
using System.Collections.Generic;

namespace QS.DbManagement {

	public abstract class ConnectionTypeBase {
		public string Title { get; protected set; }
		public string ConnectionTypeName { get; protected set; }

		public List<ConnectionParameter> Parameters { get; } = new List<ConnectionParameter>();

		public byte[] IconBytes { get; protected set; }
		
		public abstract bool CanConnect(IEnumerable<ConnectionParameterValue> parameters);

		public abstract IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null);
	}
}
