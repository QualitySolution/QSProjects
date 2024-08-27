using System.Collections.Generic;

namespace QS.DbManagement {

	public abstract class ConnectionInfo {
		public string Title { get; set; }

		public List<ConnectionParameter> Parameters { get; set; } = new List<ConnectionParameter>();

		public byte[] IconBytes { get; set; }

		public abstract IDbProvider CreateProvider();
	}
}
