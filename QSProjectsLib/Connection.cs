using System;
using QS.Configuration;

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

		#region Конструкторы
		public Connection(ConnectionType type, string name, string baseName, string server = null, string user = null, string ini = null, string account = null)
		{
			ConnectionName = name;
			BaseName = baseName;
			Server = server;
			UserName = user;
			IniName = ini;
			AccountLogin = account;
			Type = type;
		}

		public Connection(IChangeableConfiguration configuration, string section) {
			IniName = section;
			Type = (ConnectionType)int.Parse(configuration[$"{section}:Type"] ?? "0");
			ConnectionName = configuration[$"{section}:ConnectionName"];
			Server = configuration[$"{section}:Server"];
			BaseName = configuration[$"{section}:DataBase"];
			UserName = configuration[$"{section}:UserLogin"];
			AccountLogin = configuration[$"{section}:Account"];
		}
		#endregion
		
		public bool IsDemo {
			get {
				switch(Type) {
					case ConnectionType.MySQL:
						return Login.ApplicationDemoServer != null && Server == Login.ApplicationDemoServer;
					case ConnectionType.SaaS:
						return Login.ApplicationDemoAccount != null && AccountLogin == Login.ApplicationDemoAccount;
					default:
						return false;
				}
			}
		}

		public void Save(IChangeableConfiguration configuration) {
			var section = IniName;
			configuration[$"{section}:ConnectionName"] = ConnectionName;
			configuration[$"{section}:Server"] = Server;
			configuration[$"{section}:Type"] = ((int)Type).ToString ();
			configuration[$"{section}:Account"] = AccountLogin;
			configuration[$"{section}:DataBase"] = BaseName;
			configuration[$"{section}:UserLogin"] = UserName;
		}
	}
}

