using System;

namespace QSProjectsLib
{
	public enum ConnectionType {MySQL, SaaS};

	public class Connection {
		public string ConnectionName;
		public string BaseName;
		public string Server;
		public string UserName;
		public string IniName;
		public string AccountLogin;
		public ConnectionType Type;

		public Connection(ConnectionType type, string name, string baseName, string server, string user, string ini, string account)
		{
			ConnectionName = name;
			BaseName = baseName;
			Server = server;
			UserName = user;
			IniName = ini;
			AccountLogin = account;
			Type = type;
		}
	}
}

