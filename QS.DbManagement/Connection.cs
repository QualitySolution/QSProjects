using System;

namespace QS.DbManagement
{
    public class Connection : ICloneable
    {
		public ConnectionInfo ConnectionInfo { get; set; }

		public string ConnectionTitle { get; set; }

		public string User { get; set; }

		public object Clone() => new Connection {
			ConnectionInfo = (ConnectionInfo)ConnectionInfo.Clone(),
			ConnectionTitle = (string)ConnectionTitle.Clone()
		};
	}
}
