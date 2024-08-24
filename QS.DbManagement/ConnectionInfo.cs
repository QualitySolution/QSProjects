using System.Collections.Generic;

namespace QS.DbManagement {

	public class ConnectionInfo {
		public string Title { get; set; }

		public List<ConnectionParameter> Parameters { get; set; }

		public byte[] IconBytes { get; set; }

	}
}
