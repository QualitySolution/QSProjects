using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Gtk;
using MySql.Data.MySqlClient;
using QS.Configuration;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Project.Versioning;
using QS.Utilities.Text;
using QSSaaS;

namespace QSProjectsLib
{
	public partial class Login : Dialog
	{
		private readonly IChangeableConfiguration configuration;

		#region LoginSettings
		public static string ApplicationDemoServer;
		public static string ApplicationDemoAccount;
		public static string CreateDBHelpTooltip;
		public static string CreateDBHelpUrl;
		public static string OverwriteDefaultConnection;
		
		/// <summary>
		/// Функция будет вызвана для получения списка соединений по умолчанию. Вызывается если конфигурационный файл новый или не содержит настроенных соединений.
		/// Ini секция будет перезаполнена.
		/// </summary>
		public static Func<IEnumerable<Connection>> MakeDefaultConnections;
		#endregion
		#region Расширения
		public Func<IDBCreator> GetDBCreator;
		#endregion

		public List<Connection> Connections;
		String connectionError;
		public string SelectedConnection;
		public string BaseName;
		public static string DefaultBase;
		public string DemoMessage;
		private string server;
		private const bool ShowPassInException = false;
		private const int EnglishLocaleId = 1033;
		private const uint KLF_SETFORPROCESS = 0x00000100;

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Connection ConnectedTo { get; private set; } 
		
		public Gdk.Pixbuf Logo {
			set { imageLogo.Pixbuf = value; }
		}

		private String ConnectionError {
			get {
				return connectionError;
			}
			set {
				connectionError = value;
				buttonErrorInfo.Visible = !String.IsNullOrWhiteSpace(connectionError);
			}
		}

		public Login(IChangeableConfiguration configuration)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.Build();

			if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				SetKeyboardLayout(EnglishLocaleId);
			}

			SelectedConnection = String.Empty;
			Connections = new List<Connection>();
			var appInfo = new ApplicationVersionInfo();
			string ver = appInfo.Version.VersionToShortString();
			string app = appInfo.ProductTitle;
			BaseName = DefaultBase = appInfo.ProductName.ToLower();

			//Редакции
			if (!appInfo.ModificationIsHidden)
			{
				if (!String.IsNullOrEmpty(appInfo.ModificationTitle))
					app += $" {appInfo.ModificationTitle}";
				else if (!String.IsNullOrEmpty(appInfo.Modification))
					ver += $"-{appInfo.Modification}";
			}

			if (appInfo.IsBeta) {
				labelAppName.LabelProp = String.Format("<span foreground=\"gray\" size=\"larger\" font_family=\"Philosopher\"><b>{0} v.{1}</b></span>" +
				"\n<span foreground=\"gray\" font_family=\"Philosopher\">beta ({2:g})</span>",
					app, ver, appInfo.BuildDate);
			}
			else {
				labelAppName.LabelProp = String.Format("<span foreground=\"gray\" size=\"larger\" font_family=\"Philosopher\"><b>{0} v.{1}</b></span>",
					app, ver);
			}

			comboboxConnections.Clear();
			CellRendererText cell = new CellRendererText();
			comboboxConnections.PackStart(cell, false);
			comboboxConnections.AddAttribute(cell, "text", 0);

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				KeyReleaseEvent += LoginKeyReleaseEvent;
				FocusInEvent += LoginFocusInEvent;
			}
		}

		public void UpdateFromGConf()
		{
			Connections.Clear();

			foreach(var i in Enumerable.Range(-1, 100)) {
				var section = "Login" + (i >= 0 ? i.ToString() : String.Empty);
				if(configuration[$"{section}:ConnectionName"] != null)
					Connections.Add(new Connection(configuration, section));
			}

			if (Connections.Count == 0) {
				logger.Warn("Конфигурационный файл не содержит соединений. Создаем новые.");
				CreateDefaultConnections();
			}
			
			SelectedConnection = OverwriteDefaultConnection 
			                     ?? configuration["Default:ConnectionName"] 
			                     ?? Connections.FirstOrDefault()?.ConnectionName;

			entryPassword.GrabFocus();
			UpdateCombo();
		}

		protected void CreateDefaultConnections() {
			if(MakeDefaultConnections == null)
				return;
			
			int i = 0;
			foreach(var con in MakeDefaultConnections()) {
				i++;
				con.IniName = "Login" + i;
				con.Save(configuration);
				Connections.Add(con);
			}
		}

		protected virtual void OnButtonErrorInfoClicked(object sender, System.EventArgs e)
		{
			MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
								   MessageType.Error,
								   ButtonsType.Close, "ошибка");
			md.UseMarkup = false;
			md.Text = ConnectionError;
			md.Run();
			md.Destroy();
		}

		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			if (comboboxConnections.Active == -1)
				return;
			Connection Selected = ConnectedTo = Connections.Find(m => m.ConnectionName == comboboxConnections.ActiveText);
			string connStr, host;
			uint port = 3306;
			string[] uriSplit = new string[2];
			string login = entryUser.Text;

			labelLoginInfo.Text = "Соединяемся....";
			QSMain.WaitRedraw();

			if (Selected.Type == ConnectionType.MySQL) {
				Session.IsSaasConnection = false;
				uriSplit = server.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
			}
			else {
				try {
					ISaaSService svc = Session.GetSaaSService();
					string parameters = String.Format("login.{0};pass.{1};account.{2};db.{3};",
											entryUser.Text, entryPassword.Text, Selected.AccountLogin, Selected.BaseName);
					UserAuthorizeResult result = svc.authorizeUser(parameters);
					if (!result.Success) {
						labelLoginInfo.Text = "Ошибка соединения с сервисом.";
						ConnectionError = "Описание: " + result.Description;
						logger.Warn(result.Description);
						return;
					}
					uriSplit = result.Server.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
					login = result.Login;
					Session.IsSaasConnection = true;
					Session.SessionId = result.SessionID;
					Session.Account = Selected.AccountLogin;
					Session.SaasBaseName = BaseName; // Сохраняем имя базы на сервере SAAS
					BaseName = result.BaseName; //Переписываем на реальное для подключения.
				}
				catch (Exception ex) {
					labelLoginInfo.Text = "Ошибка соединения с сервисом.";
					ConnectionError = "Описание ошибки: " + ex.Message;
					logger.Warn(ex);
					return;
				}
			}
			host = uriSplit[0];
			if (uriSplit.Length > 1) {
				uint.TryParse(uriSplit[1], out port);
			}

			var conStrBuilder = new MySqlConnectionStringBuilder();
			conStrBuilder.Server = host;
			conStrBuilder.Port = port;
			conStrBuilder.Database = BaseName;
			conStrBuilder.UserID = login;
			conStrBuilder.Password = entryPassword.Text;

			connStr = conStrBuilder.GetConnectionString(true);

			QSMain.connectionDB = new MySqlConnection(connStr);
			try {
				Console.WriteLine("Connecting to MySQL...");

				QSMain.connectionDB.Open();
				string sql = "SELECT deactivated FROM users WHERE login = @login";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryUser.Text);
				using (MySqlDataReader rdr = cmd.ExecuteReader()) {
					if (rdr.Read() && DBWorks.GetBoolean(rdr, "deactivated", false) == true) {
						labelLoginInfo.Text = "Пользователь отключен.";
						ConnectionError = "Пользователь под которым вы пытаетесь войти отключен.";
						QSMain.connectionDB.Close();
						return;
					}
				}

				labelLoginInfo.Text = ConnectionError = String.Empty;
				String ini = Connections.Find(m => m.ConnectionName == comboboxConnections.ActiveText).IniName;
				configuration[$"{ini}:UserLogin"] = entryUser.Text;
				configuration["Default:ConnectionName"] = comboboxConnections.ActiveText;
				QSMain.ConnectionString = connStr;
				QSMain.ConnectionStringBuilder = conStrBuilder;
				QSMain.User = new UserInfo(entryUser.Text.ToLower());
				this.Respond(ResponseType.Ok);
			}
			catch (MySqlException ex) {
				if (ex.Number == 1045 || ex.Number == 0)
					labelLoginInfo.Text = "Доступ запрещен.\nПроверьте логин и пароль.";
				else if (ex.Number == 1042)
					labelLoginInfo.Text = "Не удалось подключиться к серверу БД.";
				else
					labelLoginInfo.Text = "Ошибка соединения с базой данных.";
				entryPassword.Text = string.Empty;
				entryPassword.GrabFocus();
				ConnectionError = "Строка соединения: " + conStrBuilder.GetConnectionString(ShowPassInException) + "\nИсключение: " + ex.ToString();
				logger.Warn(ex);
				QSMain.connectionDB.Close();
			}
			catch (AggregateException exception) when(exception.InnerException is SocketException) {
				logger.Error(exception);
				var interactive = new GtkMessageDialogsInteractive();
				interactive.ShowMessage(ImportanceLevel.Error, exception.InnerException.Message, "Ошибка соединения");
			}
		}

		protected virtual void OnEntryPasswordActivated(object sender, System.EventArgs e)
		{
			buttonOk.Activate();
		}

		protected void OnEntryActivated(object sender, EventArgs e)
		{
			this.ChildFocus(DirectionType.TabForward);
		}

		protected void OnButtonDemoClicked(object sender, EventArgs e)
		{
			MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
								   MessageType.Info,
								   ButtonsType.Ok, DemoMessage);
			md.Run();
			md.Destroy();
		}

		protected void UpdateCombo()
		{
			ListStore store = new ListStore(typeof(string));
			comboboxConnections.Model = store;
			foreach (Connection c in Connections)
				store.AppendValues(c.ConnectionName);
			SelectedConnection = configuration["Default:ConnectionName"];
			
			if (Connections.Any(m => m.ConnectionName == SelectedConnection)) {
				store.GetIterFirst(out TreeIter tempIter);
				do {
					if ((string)store.GetValue(tempIter, 0) == SelectedConnection) {
						comboboxConnections.SetActiveIter(tempIter);
						break;
					}
				} while (store.IterNext(ref tempIter));
			}
			else
				comboboxConnections.Active = 0;
			if (comboboxConnections.Active == -1)
				server = entryUser.Text = entryPassword.Text = "";
		}

		protected void OnComboboxConnectionsChanged(object sender, EventArgs e)
		{
			Connection Selected = Connections.Find(m => m.ConnectionName == comboboxConnections.ActiveText);
			server = Selected?.Server;
			entryUser.Text = Selected?.UserName;
			BaseName = Selected?.BaseName;
			SelectedConnection = Selected?.ConnectionName;
			entryPassword.GrabFocus();
			buttonDemo.Visible = Selected?.IsDemo ?? false;
		}

		protected void OnButtonEditConnectionClicked(object sender, EventArgs e)
		{
			EditConnection dlg = new EditConnection(configuration, Connections, SelectedConnection, GetDBCreator?.Invoke());
			dlg.EditingDone += (se, ev) => UpdateFromGConf();
			dlg.Run();
			dlg.Destroy();
		}

		private void LoginKeyReleaseEvent(object o, KeyReleaseEventArgs args)
		{
			CheckKeyboardState();
		}

		private void LoginFocusInEvent(object o, FocusInEventArgs args)
		{
			CheckKeyboardState();
		}

		private void CheckKeyboardState()
		{
			int capsLock = 0x14;
			IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
			uint process = NativeMethods.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
			int keyboardLayout = NativeMethods.GetKeyboardLayout(process).ToInt32() & 0xFFFF;
			labelKeyboardLayoutInfo.Visible = keyboardLayout != EnglishLocaleId;
			labelCapslockInfo.Visible = NativeMethods.GetKeyState(capsLock) != 0;
		}

		private void SetKeyboardLayout(int localeId) 
		{
			var pwszKlid = localeId.ToString("x8");
			var hkl = NativeMethods.LoadKeyboardLayout(pwszKlid, KLF_SETFORPROCESS);
			NativeMethods.ActivateKeyboardLayout(hkl, KLF_SETFORPROCESS);
		}
	}
	
	internal class NativeMethods
	{
		[DllImport("user32.dll")]
		internal static extern IntPtr GetKeyboardLayout(uint idThread);
		[DllImport("user32.dll")]
		internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr process);
		[DllImport("user32.dll")]
		internal static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll")]
		internal static extern short GetKeyState(int keyCode);
		[DllImport("user32.dll")]
		internal static extern uint LoadKeyboardLayout(string pwszKLID, uint Flags);
		[DllImport("user32.dll")]
		internal static extern uint ActivateKeyboardLayout(uint hkl, uint Flags);
	}
}
