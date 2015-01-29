using System;
using System.Reflection;
using Gtk;
using Nini.Config;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QSSaaS;
using System.ServiceModel.Web;
using System.ServiceModel;

namespace QSProjectsLib
{
	public partial class Login : Gtk.Dialog
	{
		public List<Connection> Connections;
		String ConnectionError;
		public string SelectedConnection;
		public string BaseName;
		public string ConfigFileName;
		public string DefaultLogin;
		public string DefaultServer;
		public string DefaultConnection;
		public string DemoServer;
		public string DemoMessage;
		private string server;
		IniConfigSource Configsource;

		public Gdk.Pixbuf Logo {
			set{ imageLogo.Pixbuf = value; }
		}

		public Login ()
		{
			this.Build ();
			QSSaaS.Session.SaaSService = "localhost:8080/SAS";
			SelectedConnection = String.Empty;
			Connections = new List<Connection> ();
			DefaultServer = "localhost";
			System.Reflection.Assembly ass = Assembly.GetCallingAssembly ();
			Version version = ass.GetName ().Version;
			string ver = version.ToString (version.Revision == 0 ? (version.Build == 0 ? 2 : 3) : 4);
			object[] att = ass.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);
			string app = ((AssemblyTitleAttribute)att [0]).Title;
			labelAppName.LabelProp = String.Format ("<span foreground=\"gray\" size=\"large\" font_family=\"FifthLeg\">{0} v.{1}</span>",
			                                        app, ver);
			comboboxConnections.Clear ();
			CellRendererText cell = new CellRendererText ();
			comboboxConnections.PackStart (cell, false);
			comboboxConnections.AddAttribute (cell, "text", 0);
		}

		public void SetDefaultNames (string ProjectName)
		{
			BaseName = ProjectName;
			DefaultConnection = "По умолчанию";
			ConfigFileName = ProjectName + ".ini";
		}

		public void UpdateFromGConf ()
		{
			string configfile = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), ConfigFileName);
			IConfig Config;
			try {
				Configsource = new IniConfigSource (configfile);
				Configsource.Reload ();  //Читаем все конфиги
				System.Collections.IEnumerator en = Configsource.Configs.GetEnumerator ();
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
				Configsource = new IniConfigSource ();		
				CreateDefaultConnection (configfile);
			} finally {
				if (Configsource.Configs ["Default"] == null) {     //Создаем раздел по-умолчанию, чтобы он гарантировано был
					Configsource.AddConfig ("Default");
					Configsource.Configs ["Default"].Set ("ConnectionName", String.Empty);
					Configsource.Save ();
				}
				entryPassword.GrabFocus ();
				UpdateCombo ();
			}
		}

		protected void CreateDefaultConnection (string configfile)
		{
			IConfig config = Configsource.AddConfig ("Login");
			config.Set ("UserLogin", DefaultLogin);
			config.Set ("Server", DefaultServer);
			config.Set ("ConnectionName", DefaultConnection);
			if (Configsource.Configs ["Default"] == null)
				Configsource.AddConfig ("Default");
			Configsource.Configs ["Default"].Set ("ConnectionName", DefaultConnection);

			Configsource.Save (configfile);
		
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
			Connection Selected = Connections.Find (m => m.ConnectionName == comboboxConnections.ActiveText);
			string connStr = String.Empty, host = "localhost", port = "3306";
			string[] uriSplit = new string[2];
			string login = entryUser.Text;

			if (Selected.Type == ConnectionType.MySQL) {
				QSMain.Use_SaaS = false;
				uriSplit = server.Split (new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
			} else {
				try {
					Uri address = new Uri (QSSaaS.Session.SaaSService);
					var factory = new WebChannelFactory<ISaaSService> (new WebHttpBinding { AllowCookies = true }, address);
					ISaaSService svc = factory.CreateChannel ();
					UserAuthResult result = svc.authUser (entryUser.Text, entryPassword.Text, Selected.AccountLogin, Selected.BaseName);
					if (!result.Success) {
						labelLoginInfo.Text = "Ошибка соединения с сервисом.";
						buttonErrorInfo.Visible = true;
						ConnectionError = "Описание: " + result.Description;
						Console.WriteLine (result.Description);
						return;
					}
					uriSplit = result.Server.Split (new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
					login = result.Login;
					QSMain.Use_SaaS = true;
					QSMain.Session_ID = result.SessionID;
					factory.Close();
				} catch (Exception ex) {
					labelLoginInfo.Text = "Ошибка соединения с сервисом.";
					buttonErrorInfo.Visible = true;
					ConnectionError = "Описание ошибки: " + ex.Message;
					Console.WriteLine (ex.Message);
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
				labelLoginInfo.Text = "Соединяемся....";
				while (GLib.MainContext.Pending ()) {
					Gtk.Main.Iteration ();
				}
				
				QSMain.connectionDB.Open ();
				labelLoginInfo.Text = "";
				buttonErrorInfo.Visible = false;
				String ini = Connections.Find (m => m.ConnectionName == comboboxConnections.ActiveText).IniName;
				Configsource.Configs [ini].Set ("UserLogin", entryUser.Text);
				Configsource.Configs ["Default"].Set ("ConnectionName", comboboxConnections.ActiveText);
				Configsource.Save ();
				QSMain.ConnectionString = connStr;
				QSMain.User.Login = entryUser.Text.ToLower ();
				this.Respond (ResponseType.Ok);
			} catch (MySqlException ex) {
				if (ex.Number == 1045)
					labelLoginInfo.Text = "Доступ запрещен.\nПроверьте логин и пароль.";
				else
					labelLoginInfo.Text = "Ошибка соединения с базой данных.";
				buttonErrorInfo.Visible = true;
				ConnectionError = "Строка соединения: " + connStr + "\nИсключение: " + ex.ToString ();
				Console.WriteLine (ex.ToString ());
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
			SelectedConnection = (String)Configsource.Configs ["Default"].Get ("ConnectionName", String.Empty);
			if (SelectedConnection != String.Empty) {
				if (Connections.Find (m => m.ConnectionName == SelectedConnection) == null) {
					Configsource.Configs ["Default"].Set ("ConnectionName", String.Empty);
					Configsource.Save ();
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
				comboboxConnections.Active = -1;
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
			EditConnection dlg = new EditConnection (ref Connections, ref Configsource);
			dlg.Run ();
			dlg.Destroy ();
			UpdateCombo ();
		}
	}
}