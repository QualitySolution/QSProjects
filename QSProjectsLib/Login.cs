using System;
using Gtk;
using Nini.Config;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace QSProjectsLib
{
	public partial class Login : Gtk.Dialog
	{
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
		}

		public void SetDefaultNames(string ProjectName)
		{
			BaseName = ProjectName;
			ConfigFileName = ProjectName + ".ini";
		}
		
		public void UpdateFromGConf ()
		{
			string configfile = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), ConfigFileName);
			try
			{
				Configsource = new IniConfigSource(configfile);
				Configsource.Reload();
				entryServer.Text = Configsource.Configs["Login"].Get("Server");
				entryUser.Text = Configsource.Configs["Login"].Get("UserLogin");
				entryPassword.GrabFocus();
				BaseName = Configsource.Configs["Login"].Get("DataBase", BaseName);
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
				Configsource.Configs["Login"].Set("Server", entryServer.Text);
				Configsource.Configs["Login"].Set("UserLogin", entryUser.Text);
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
	}
}