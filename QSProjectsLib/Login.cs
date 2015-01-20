using System;
using System.Reflection;
using Gtk;
using Nini.Config;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace QSProjectsLib
{
	public class Connection {
		public string ConnectionName;
		public string BaseName;
		public string Server;
		public string UserName;
		public string IniName;

		public Connection(string name, string baseName, string server, string user, string ini)
		{
			ConnectionName = name;
			BaseName = baseName;
			Server = server;
			UserName = user;
			IniName = ini;
		}
	}

	public partial class Login : Gtk.Dialog
	{
		public List<Connection> Connections = new List<Connection>();
		String ConnectionError;
		public string BaseName;
		public string ConfigFileName;
		public string DefaultLogin;
		public string DefaultServer;
		public string DemoServer;
		public string DemoMessage;
		IniConfigSource Configsource;

		public Gdk.Pixbuf Logo {
			set{imageLogo.Pixbuf = value;}
		}

		public Login ()
		{
			this.Build ();
			DefaultServer = "localhost";
			System.Reflection.Assembly ass = Assembly.GetCallingAssembly();
			Version version = ass.GetName().Version;
			string ver = version.ToString (version.Revision == 0 ? (version.Build == 0 ? 2 : 3) : 4);
			object[] att = ass.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);
			string app = ((AssemblyTitleAttribute)att [0]).Title;
			labelAppName.LabelProp = String.Format ("<span foreground=\"gray\" size=\"large\" font_family=\"FifthLeg\">{0} v.{1}</span>",
			                                        app, ver);
			entryServer.Sensitive = false;

			comboboxConnections.Clear();
			CellRendererText cell = new CellRendererText();
			comboboxConnections.PackStart(cell, false);
			comboboxConnections.AddAttribute(cell, "text", 0);

		}

		public void SetDefaultNames(string ProjectName)
		{
			BaseName = ProjectName;
			ConfigFileName = ProjectName + ".ini";
		}
		
		public void UpdateFromGConf ()
		{
			string configfile = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), ConfigFileName);
			IniConfig Config;
			try
			{
				Configsource = new IniConfigSource(configfile);
				Configsource.Reload();
				Config = (IniConfig)Configsource.Configs["Login"];
				Connections.Add(new Connection(Config.Get("ConnectionName", "Соединение 1"),
				                               Config.Get("DataBase", BaseName),
				                               Config.Get("Server"),
				                               Config.Get("UserLogin"),
				                               "Login"));
				for (int i = 0; i < Configsource.Configs.Count; i++) {
					if ((Config = (IniConfig)Configsource.Configs["Login" + i.ToString()]) != null)
						Connections.Add(new Connection(Config.Get("ConnectionName", "Без имени " + i.ToString()),
						                               Config.Get("DataBase", BaseName),
					                                   Config.Get("Server"),
					                                   Config.Get("UserLogin"),
						                               "Login" + i.ToString()));
				}
				entryPassword.GrabFocus();
				UpdateCombo();
			} 
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine("Конфигурационный фаил не найден. Создаем новый.");
				Configsource = new IniConfigSource();
				
				IConfig config = Configsource.AddConfig("Login");
				config.Set("UserLogin", DefaultLogin);
				config.Set("Server", DefaultServer);
				Configsource.Save(configfile);
				
				entryServer.Text = config.Get("Server");
				entryUser.Text = config.Get("UserLogin");
			}
		}
		
		protected virtual void OnButtonErrorInfoClicked (object sender, System.EventArgs e)
		{
			MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent,
			                                      MessageType.Error, 
			                                      ButtonsType.Close,"ошибка");
			md.UseMarkup = false;
			md.Text = ConnectionError;
			md.Run ();
			md.Destroy();
		}
		
		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
            string host = "localhost";
            string port = "3306";
            string[] uriSplit = entryServer.Text.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            host = uriSplit[0];
            if (uriSplit.Length > 1)
            {
                port = uriSplit[1];
            }

			string connStr = "server=" + host + ";user=" +
				entryUser.Text + ";database=" + BaseName + ";port=" + port + ";password=" +
					entryPassword.Text + ";";
			QSMain.connectionDB = new MySqlConnection(connStr);
			try
			{
				Console.WriteLine("Connecting to MySQL...");
				labelLoginInfo.Text = "Соединяемся....";
				while (GLib.MainContext.Pending())
				{
					Gtk.Main.Iteration();
				}
				
				QSMain.connectionDB.Open();
				labelLoginInfo.Text = "";
				buttonErrorInfo.Visible = false;
				String ini = Connections.Find(m => m.ConnectionName == comboboxConnections.ActiveText).IniName;
				Configsource.Configs[ini].Set("UserLogin", entryUser.Text);
				Configsource.Configs["Default"].Set("Name", comboboxConnections.ActiveText);
				Configsource.Save();
				QSMain.ConnectionString = connStr;
				QSMain.User.Login = entryUser.Text.ToLower();
				this.Respond(ResponseType.Ok);
			}
			catch (MySqlException ex)
			{
				if(ex.Number == 1045)
					labelLoginInfo.Text = "Доступ запрещен.\nПроверьте логин и пароль.";
				else
					labelLoginInfo.Text = "Ошибка соединения с базой данных.";
				buttonErrorInfo.Visible = true;
				ConnectionError = "Строка соединения: " + connStr + "\nИсключение: " + ex.ToString();
				Console.WriteLine(ex.ToString());
				QSMain.connectionDB.Close();
			}
			
		}
		
		protected virtual void OnEntryPasswordActivated (object sender, System.EventArgs e)
		{
			buttonOk.Activate();
		}
		
		protected void OnEntryActivated (object sender, EventArgs e)
		{
			this.ChildFocus (DirectionType.TabForward);
		}
		
		protected void OnEntryServerChanged (object sender, EventArgs e)
		{
			if(DemoServer == null)
				return;
			buttonDemo.Visible = entryServer.Text.ToLower() == DemoServer;
		}
		
		protected void OnButtonDemoClicked (object sender, EventArgs e)
		{
			MessageDialog md = new MessageDialog( this, DialogFlags.DestroyWithParent,
			                                     MessageType.Info, 
			                                     ButtonsType.Ok, DemoMessage);
			md.Run ();
			md.Destroy();
		}

		protected void UpdateCombo()
		{
			ListStore store = new ListStore(typeof (string));
			comboboxConnections.Model = store;
			foreach (Connection c in Connections)
				store.AppendValues(c.ConnectionName);
		}

		protected void OnComboboxConnectionsChanged (object sender, EventArgs e)
		{
			Connection Selected = Connections.Find(m => m.ConnectionName == comboboxConnections.ActiveText);
			entryServer.Text = Selected.Server;
			entryUser.Text = Selected.UserName;
			BaseName = Selected.BaseName;
			entryPassword.GrabFocus();
		}

		protected void OnButtonEditConnectionClicked (object sender, EventArgs e)
		{
			EditConnection dlg = new EditConnection (ref Connections, ref Configsource);
			dlg.Run ();
			dlg.Destroy ();
			Connections.RemoveAll (m => m == null);
			UpdateCombo ();
		}
	}
}