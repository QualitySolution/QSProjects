using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using QSSupportLib;
using Gtk;
using QSProjectsLib;
using System.Net;
using System.ComponentModel;
using NLog;
using MySql.Data.MySqlClient;
using System.Data;
using Nini.Config;
using System.ServiceModel.Channels;

namespace QSUpdater
{
	public class TestUpdate
	{
		public static Logger logger = LogManager.GetCurrentClassLogger ();
		public static IUpdateService svc;
		public static Uri address;
		public static UpdateResult res;
		public static MenuItem updMenu;
		private static ProgressBar updPr;
		public static Window updWin;
		public static string updMessage;
		public static string noties = "Не проверять это обновление при запуске продукта";
		private static string tempPath = System.IO.Path.GetTempPath ();
		public static string newVersion = null;
		public static string checkVersion = String.Empty;
		public static string checkResult = String.Empty;

		private static string connectionString;
		private static string ConfigFile = @"C:/Users/Арсений/AppData/Roaming/qsserver.txt";
		private static string server;
		private static string port;
		private static string pass;
		private static string db;
		private static string user;

		private static string clientIP = null;
		public static string serialNumber = null;
		public static string edition = null;

		private static RemoteEndpointMessageProperty clientEndpoint = null;

		public static bool IsLinux {
			get {
				int p = (int)Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}

		static public void LoadUpd ()
		{
			Thread loadThread = new Thread (new ThreadStart (ThreadWorks));
			loadThread.Start ();
		}

		static void ThreadWorks ()
		{
			try {
				logger.Info ("Получаем данные от сервера");
				address = new Uri ("http://localhost:8080/Updater");
				var factory = new WebChannelFactory<IUpdateService> (new WebHttpBinding { AllowCookies = true }, address);
				svc = factory.CreateChannel ();
				res = svc.CheckUpd (MainSupport.ProjectVerion.Product, MainSupport.ProjectVerion.Edition, serialNumber, MainSupport.ProjectVerion.Version.Major, MainSupport.ProjectVerion.Version.Minor, MainSupport.ProjectVerion.Version.Build, MainSupport.ProjectVerion.Version.Revision); 

				Application.Invoke (delegate {
					newVersion = res.NewVersion;
					if (QSMain.Configsource.Configs ["Updater"] != null) {
						checkVersion = QSMain.Configsource.Configs ["Updater"].Get ("NewVersion", String.Empty);
						checkResult = QSMain.Configsource.Configs ["Updater"].Get ("Check", String.Empty);
					}
					if (res.HasUpdate) {
						if (checkResult == "True" || checkResult == String.Empty) {
							ShowDialog ();
						} else if (checkVersion != newVersion)
							ShowDialog ();
					} 
				}
				);
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка доступа к серверу обновления.", ex);
			}
		}

		public static void ShowDialog ()
		{
			try {
				if (res.HasUpdate) {
					updMessage = string.Format ("<b>\nДоступна новая версия программы " + res.NewVersion + ". Мы предлагаем вам ее загрузить и установить.</b>" +
					"\n\nВаша версия программного продукта: " + MainSupport.ProjectVerion.Version + "." +
					"\n\n<b>Вы хотите скачать и установить новую версию?</b>\n\n");
				    
					if (res.UpdateDescription != String.Empty)
						updMessage = string.Format ("<b>\nДоступна новая версия программы: " + res.NewVersion + ". Мы предлагаем вам ее загрузить и установить.</b>" +
						"\n\nВаша версия программного продукта: " + MainSupport.ProjectVerion.Version + ".\n\nИнформация об обновлении:\n" + res.UpdateDescription +
						"\n\n<b>Вы хотите скачать и установить новую версию?</b>\n\n");

				} else
					//res.NewVersion = null;
					updMessage = string.Format ("<b>\nВаша версия программного продукта: " + MainSupport.ProjectVerion.Version + ".</b>" +
					"\n\nНа данный момент это актуальная версия продукта.\n");

				UpdaterDialog updDlg = new UpdaterDialog ();
				updDlg.Resizable = false;
				int result = updDlg.Run ();
				updDlg.Destroy ();

				if ((ResponseType)result == ResponseType.Ok) {
					logger.Info("Скачивание обновления началось.");
					updWin = new Window ("Подождите...");
					updWin.SetSizeRequest (300, 25); 
					updWin.Resizable = false;
					updWin.SetPosition (WindowPosition.Center);
					updPr = new ProgressBar ();
					VBox vbox = new VBox ();
					updWin.Add (vbox); 
					updPr.Text = "Новая версия скачивается, подождите...";
					vbox.PackStart (updPr, true, true, 0);

					updWin.ShowAll ();

					WebClient webClient = new WebClient ();
					webClient.DownloadFileCompleted += new AsyncCompletedEventHandler (Completed);
					webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler (ProgressChanged);
					webClient.DownloadFileAsync (new Uri (res.FileLink), tempPath + @"\QSInstaller.exe");

				} else if ((ResponseType)result == ResponseType.Cancel)
				if (res.HasUpdate)
					ConfigFileUpdater ();
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка доступа к серверу обновления.", ex);
				updMessage = "Извините, сервер обновления не работает";
				Window win = new Window ("");
				MessageDialog md = new MessageDialog (win, DialogFlags.DestroyWithParent,
				                                      MessageType.Error, 
				                                      ButtonsType.Ok,
				                                      updMessage);
				md.Run ();
				md.Destroy ();
			}
		}

		protected static void ConfigFileUpdater ()
		{
			if (UpdaterDialog.updChecker) {

				if (QSMain.Configsource.Configs ["Updater"] == null) {
					QSMain.Configsource.AddConfig ("Updater");
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "False");
					QSMain.Configsource.Save ();
				} else {
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "False");
					QSMain.Configsource.Save ();
				}
			} else {
				if (QSMain.Configsource.Configs ["Updater"] == null) {
					QSMain.Configsource.AddConfig ("Updater");
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "True");
					QSMain.Configsource.Save ();
				} else {
					QSMain.Configsource.Configs ["Updater"].Set ("NewVersion", UpdaterDialog.checkVersion);
					QSMain.Configsource.Configs ["Updater"].Set ("Check", "True");
					QSMain.Configsource.Save ();
				}
			}
		}

		protected static void ProgressChanged (object sender, DownloadProgressChangedEventArgs e)
		{
			Application.Invoke (delegate {
				updPr.Fraction = e.ProgressPercentage / 100.0;
			});
		}

		protected static void Completed (object sender, AsyncCompletedEventArgs e)
		{
			logger.Info ("Скачивание обновления завершено.");
			if (updWin.IsMapped) {
				StartProcess ();
				Application.Quit ();
			}
		}

		protected static void StartProcess ()
		{
			logger.Info ("Запускаем файл установки обновления.");
			System.Diagnostics.Process File = new System.Diagnostics.Process ();
			File.StartInfo.FileName = tempPath + @"\QSInstaller.exe";
			updWin.Destroy ();
			File.Start ();
		}

		public static void EntryStatistics ()
		{
			if (IsLinux) {
				clientEndpoint = OperationContext.Current.IncomingMessageProperties [RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
				//clientName = System.Net.Dns.GetHostEntry (clientEndpoint.Address).HostName;
				clientIP = clientEndpoint.Address;
			}
			try {
				IniConfigSource configFile = new IniConfigSource (ConfigFile);
				configFile.Reload ();	
				IConfig config = configFile.Configs ["General"];
				server = config.GetString ("server");
				port = config.GetString ("port", "3306");
				pass = config.GetString ("password");
				db = config.GetString ("database");
				user = config.GetString ("user");
			} catch (Exception ex) {
				logger.FatalException ("Ошибка чтения конфигурационного файла.", ex);
				return;
			}
			connectionString = "server=" + server + ";user=" + user + ";database=" + db + ";port=" + port + ";password=" + pass + ";";
		
			string sql;
			if (MainSupport.ProjectVerion.Edition != String.Empty) {
				edition = MainSupport.ProjectVerion.Edition;
			}
			MySqlConnection connection = new MySqlConnection (connectionString);

			sql = "INSERT INTO QSService.statistics (product, edition, serial_number, client_version, new_version, date, client_ip) " +
			"VALUES (@product, @edition, @serial_number, @client_version, @new_version, @date, @client_ip)";
		
			logger.Info ("Запись обновления");
			connection.Open ();
			try {
				MySqlCommand cmd = new MySqlCommand (sql, connection);

				cmd.Parameters.AddWithValue ("@product", MainSupport.ProjectVerion.Product);
				cmd.Parameters.AddWithValue ("@edition", edition);
				cmd.Parameters.AddWithValue ("@client_version", MainSupport.ProjectVerion.Version);
				cmd.Parameters.AddWithValue ("@new_version", newVersion);
				cmd.Parameters.AddWithValue ("@date", DateTime.Now);
				cmd.Parameters.AddWithValue ("@serial_number", serialNumber);
				cmd.Parameters.AddWithValue ("@client_ip", clientIP);

				cmd.ExecuteNonQuery ();
				logger.Info ("Ok");

			} catch (Exception ex) {
				logger.ErrorException ("Ошибка записи статистики обновления.", ex);
			} finally {
				if (connection.State != ConnectionState.Closed)
					connection.Close ();
			}
		}
	}
}

