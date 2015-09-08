using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Gtk;
using MySql.Data.MySqlClient;
using Nini.Config;
using QSSaaS;

namespace QSProjectsLib
{
	public partial class Login : Dialog
	{
		public List<Connection> Connections;
		String connectionError;
		public string SelectedConnection;
		public string BaseName;
		public string DefaultLogin;
		public string DefaultServer;
		public static string DefaultBase;
		public string DefaultConnection;
		public string DemoServer;
		public string DemoMessage;
		private string server;

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Gdk.Pixbuf Logo {
			set { imageLogo.Pixbuf = value; }
		}

		private String ConnectionError {
			get {
				return connectionError;
			}
			set {
				connectionError = value;
				buttonErrorInfo.Visible = !String.IsNullOrWhiteSpace (connectionError);
			}
		}

		public Login ()
		{
			this.Build ();
			SelectedConnection = String.Empty;
			Connections = new List<Connection> ();
			DefaultServer = "localhost";
			Assembly ass = Assembly.GetCallingAssembly ();
			Version version = ass.GetName ().Version;
			string ver = version.ToString (version.Revision == 0 ? (version.Build == 0 ? 2 : 3) : 4);
			object[] att = ass.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);
			string app = ((AssemblyTitleAttribute)att [0]).Title;

			object[] betaAtt = ass.GetCustomAttributes (typeof(AssemblyBetaBuildAttribute), false);
			if(betaAtt.Length > 0)
			{
				var buildDate = System.IO.File.GetLastWriteTime(ass.Location);
				labelAppName.LabelProp = String.Format ("<span foreground=\"gray\" size=\"larger\" font_family=\"Philosopher\"><b>{0} v.{1}</b></span>" +
					"\n<span foreground=\"gray\" font_family=\"Philosopher\">beta ({2:g})</span>",
					app, ver, buildDate);
			}
			else
			{
				labelAppName.LabelProp = String.Format ("<span foreground=\"gray\" size=\"larger\" font_family=\"Philosopher\"><b>{0} v.{1}</b></span>",
					app, ver);
			}	

			comboboxConnections.Clear ();
			CellRendererText cell = new CellRendererText ();
			comboboxConnections.PackStart (cell, false);
			comboboxConnections.AddAttribute (cell, "text", 0);
		}

		public void SetDefaultNames (string ProjectName)
		{
			BaseName = DefaultBase = ProjectName;
			DefaultConnection = "По умолчанию";
			QSMain.ConfigFileName = ProjectName + ".ini";
		}

		public void UpdateFromGConf ()
		{
			string configfile = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), QSMain.ConfigFileName);
			IConfig Config;
			try {
				QSMain.Configsource = new IniConfigSource (configfile);
				QSMain.Configsource.Reload ();  //Читаем все конфиги
				System.Collections.IEnumerator en = QSMain.Configsource.Configs.GetEnumerator ();
				while (en.MoveNext ()) {
					Config = (IConfig)en.Current;
					if (Regex.IsMatch (Config.Name, @"Login[0-9]*")) {
						String type = Config.Get ("Type", ((int)ConnectionType.MySQL).ToString ());
						Connections.Add (new Connection ((ConnectionType)int.Parse (type),
						                                 Config.Get ("ConnectionName", DefaultConnection),
						                                 Config.Get ("DataBase", BaseName),
						                                 Config.Get ("Server", DefaultServer),
						                                 Config.Get ("UserLogin", String.Empty),
						                                 Config.Name,
						                                 Config.Get ("Account", String.Empty)));
					} else if (Config.Name == "Default") {
						SelectedConnection = Config.Get ("ConnectionName", String.Empty);
					}
				}
				if (Connections.Count == 0) {
					Console.WriteLine ("Конфигурационный файл не содержит соединений. Создаем новое.");
					CreateDefaultConnection (configfile);
				}
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine ("Конфигурационный фаил не найден. Создаем новый.");
				QSMain.Configsource = new IniConfigSource ();		
				CreateDefaultConnection (configfile);
			} finally {
				if (QSMain.Configsource.Configs ["Default"] == null) {     //Создаем раздел по-умолчанию, чтобы он гарантировано был
					QSMain.Configsource.AddConfig ("Default");
					QSMain.Configsource.Configs ["Default"].Set ("ConnectionName", String.Empty);
					QSMain.Configsource.Save ();
				}
				entryPassword.GrabFocus ();
				UpdateCombo ();
			}
		}

		protected void CreateDefaultConnection (string configfile)
		{
			IConfig config = QSMain.Configsource.AddConfig ("Login");
			config.Set ("UserLogin", DefaultLogin);
			config.Set ("Server", DefaultServer);
			config.Set ("ConnectionName", DefaultConnection);
			if (QSMain.Configsource.Configs ["Default"] == null)
				QSMain.Configsource.AddConfig ("Default");
			QSMain.Configsource.Configs ["Default"].Set ("ConnectionName", DefaultConnection);

			QSMain.Configsource.Save (configfile);
		
			Connections.Add (new Connection (ConnectionType.MySQL, DefaultConnection, BaseName, DefaultServer, DefaultLogin, "", ""));

			server = config.Get ("Server");
			entryUser.Text = config.Get ("UserLogin");
			SelectedConnection = DefaultConnection;
		}

		protected virtual void OnButtonErrorInfoClicked (object sender, System.EventArgs e)
		{
			MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent,
			                                      MessageType.Error, 
			                                      ButtonsType.Close, "ошибка");
			md.UseMarkup = false;
			md.Text = ConnectionError;
			md.Run ();
			md.Destroy ();
		}

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			if (comboboxConnections.Active == -1)
				return;
			Connection Selected = Connections.Find (m => m.ConnectionName == comboboxConnections.ActiveText);
			string connStr, host, port = "3306";
			string[] uriSplit = new string[2];
			string login = entryUser.Text;

			labelLoginInfo.Text = "Соединяемся....";
			QSMain.WaitRedraw ();

			if (Selected.Type == ConnectionType.MySQL) {
				Session.IsSaasConnection = false;
				uriSplit = server.Split (new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
			} else {
				try {
					ISaaSService svc = Session.GetSaaSService ();
					string parameters = String.Format ("login.{0};pass.{1};account.{2};db.{3};",
					                                   entryUser.Text, entryPassword.Text, Selected.AccountLogin, Selected.BaseName); 
					UserAuthorizeResult result = svc.authorizeUser (parameters);
					if (!result.Success) {
						labelLoginInfo.Text = "Ошибка соединения с сервисом.";
						ConnectionError = "Описание: " + result.Description;
						logger.Warn (result.Description);
						return;
					}
					uriSplit = result.Server.Split (new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
					login = result.Login;
					Session.IsSaasConnection = true;
					Session.SessionId = result.SessionID;
					Session.Account = Selected.AccountLogin;
					Session.SaasBaseName = BaseName;
					Session.SQLBaseName = result.BaseName;
				} catch (Exception ex) {
					labelLoginInfo.Text = "Ошибка соединения с сервисом.";
					ConnectionError = "Описание ошибки: " + ex.Message;
					logger.Warn (ex);
					return;
				}
			}
			host = uriSplit [0];
			if (uriSplit.Length > 1) {
				port = uriSplit [1];
			}

			connStr = string.Format ("server={0};user={1};database={2};port={3};password={4};", host, login, BaseName, port, entryPassword.Text);
			
			QSMain.connectionDB = new MySqlConnection (connStr);
			try {
				Console.WriteLine ("Connecting to MySQL...");
				
				QSMain.connectionDB.Open ();
				string sql = "SELECT deactivated FROM users WHERE login = @login";
				try
				{
					MySqlCommand cmd = new MySqlCommand (sql, QSMain.connectionDB);
					cmd.Parameters.AddWithValue ("@login", entryUser.Text);
					using(MySqlDataReader rdr = cmd.ExecuteReader ())
					{
						if (rdr.Read () && DBWorks.GetBoolean (rdr, "deactivated", false) == true) {
							labelLoginInfo.Text = "Пользователь деактивирован.";
							ConnectionError = "Пользователь под которым вы пытаетесь войти, деактивирован в настройках базы.";
							QSMain.connectionDB.Close ();
							return;
						}
					}
				} catch(MySqlException myEx)
				{
					if(myEx.Number == 1054)
						logger.Warn (myEx, "Возможно не установлен микро-обновление, пропускаем проверку отключен ли пользователь.");
					else
						throw myEx;
				}

				labelLoginInfo.Text = ConnectionError = String.Empty;
				String ini = Connections.Find (m => m.ConnectionName == comboboxConnections.ActiveText).IniName;
				QSMain.Configsource.Configs [ini].Set ("UserLogin", entryUser.Text);
				QSMain.Configsource.Configs ["Default"].Set ("ConnectionName", comboboxConnections.ActiveText);
				QSMain.Configsource.Save ();
				QSMain.ConnectionString = connStr;
				QSMain.User = new UserInfo (entryUser.Text.ToLower ());
				this.Respond (ResponseType.Ok);
			} catch (MySqlException ex) {
				if (ex.Number == 1045)
					labelLoginInfo.Text = "Доступ запрещен.\nПроверьте логин и пароль.";
				else
					labelLoginInfo.Text = "Ошибка соединения с базой данных.";

				ConnectionError = "Строка соединения: " + connStr + "\nИсключение: " + ex.ToString ();
				logger.Warn (ex);
				QSMain.connectionDB.Close ();
			}
			
		}

		protected virtual void OnEntryPasswordActivated (object sender, System.EventArgs e)
		{
			buttonOk.Activate ();
		}

		protected void OnEntryActivated (object sender, EventArgs e)
		{
			this.ChildFocus (DirectionType.TabForward);
		}

		protected void OnButtonDemoClicked (object sender, EventArgs e)
		{
			MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent,
			                                      MessageType.Info, 
			                                      ButtonsType.Ok, DemoMessage);
			md.Run ();
			md.Destroy ();
		}

		protected void UpdateCombo ()
		{
			ListStore store = new ListStore (typeof(string));
			comboboxConnections.Model = store;
			foreach (Connection c in Connections)
				store.AppendValues (c.ConnectionName);
			SelectedConnection = (String)QSMain.Configsource.Configs ["Default"].Get ("ConnectionName", String.Empty);
			if (SelectedConnection != String.Empty) {
				if (Connections.Find (m => m.ConnectionName == SelectedConnection) == null) {
					QSMain.Configsource.Configs ["Default"].Set ("ConnectionName", String.Empty);
					QSMain.Configsource.Save ();
					SelectedConnection = String.Empty;
				} else {
					TreeIter tempIter;
					store.GetIterFirst (out tempIter);
					do {
						if ((string)store.GetValue (tempIter, 0) == SelectedConnection) {
							comboboxConnections.SetActiveIter (tempIter);
							break;
						}
					} while (store.IterNext (ref tempIter));
				}
			} else
				comboboxConnections.Active = 0;
			if (comboboxConnections.Active == -1)
				server = entryUser.Text = entryPassword.Text = "";
		}

		protected void OnComboboxConnectionsChanged (object sender, EventArgs e)
		{
			Connection Selected = Connections.Find (m => m.ConnectionName == comboboxConnections.ActiveText);
			server = Selected.Server;
			entryUser.Text = Selected.UserName;
			BaseName = Selected.BaseName;
			entryPassword.GrabFocus ();
			if (DemoServer == null)
				return;
			buttonDemo.Visible = server.ToLower () == DemoServer;
		}

		protected void OnButtonEditConnectionClicked (object sender, EventArgs e)
		{
			EditConnection dlg = new EditConnection (ref Connections, ref QSMain.Configsource);
			dlg.Run ();
			dlg.Destroy ();
			UpdateCombo ();
		}
	}
}