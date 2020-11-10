using System;
using System.Data.Common;
using System.IO;
using MySql.Data.MySqlClient;
using QS.Dialog;
using QS.Project.Versioning;
using QS.Utilities.Text;

namespace QS.Updater.DB
{
	/// <summary>
	/// Пока просто класс адаптера, чтобы не рефакторить основной модуль который в этом нуждается.
	/// </summary>
	public class SQLDBUpdater : IDBUpdater
	{
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly UpdateConfiguration configuration;
		private readonly dynamic parametersService;
		private readonly IApplicationInfo applicationInfo;
		private readonly DbConnection connection;
		private readonly IDBChangePermission permission;
		private readonly IInteractiveMessage interactiveMessage;
		private readonly CheckBaseVersion checkBaseVersion;

		public SQLDBUpdater(UpdateConfiguration configuration, BaseParameters.ParametersService parametersService, IApplicationInfo applicationInfo, DbConnection connection, IDBChangePermission permission, IInteractiveMessage interactiveMessage, CheckBaseVersion checkBaseVersion)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.permission = permission ?? throw new ArgumentNullException(nameof(permission));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.checkBaseVersion = checkBaseVersion;
		}

		public void CheckMicroUpdatesDB()
		{
			Version currentDB = new Version();
			if(parametersService.micro_updates != null)
				currentDB = Version.Parse(parametersService.micro_updates);

			var dbMainVersion = Version.Parse(parametersService.version);

			if(currentDB < dbMainVersion)
				currentDB = dbMainVersion;

			logger.Info("Проверяем микро обновления базы(текущая версия:{0})", VersionHelper.VersionToShortString(currentDB));
			var beforeUpdates = currentDB;

			while(configuration.MicroUpdates.Exists(u => u.Source == currentDB)) {
				var update = configuration.MicroUpdates.Find(u => u.Source == currentDB);

				if(!permission.HasPermission)
					NotAdminErrorAndExit(true, update.Source, update.Destanation);

				logger.Info("Обновляемся до {0}", VersionHelper.VersionToShortString(update.Destanation));
				var trans = connection.BeginTransaction();
				try {
					string sql;
					using(Stream stream = update.Assembly.GetManifestResourceStream(update.Resource)) {
						if(stream == null)
							throw new InvalidOperationException(String.Format("Ресурс {0} указанный в обновлениях не найден.", update.Resource));
						StreamReader reader = new StreamReader(stream);
						sql = reader.ReadToEnd();
					}

					var cmd = connection.CreateCommand();
					cmd.CommandText = sql;
					cmd.Transaction = trans;
					cmd.ExecuteNonQuery();
					trans.Commit();
					currentDB = update.Destanation;
				}
				catch(Exception ex) {
					trans.Rollback();
					throw ex;
				}
			}

			if(currentDB != beforeUpdates) {
				parametersService.micro_updates = VersionHelper.VersionToShortString(currentDB);

				interactiveMessage.ShowMessage(ImportanceLevel.Info, String.Format("Выполнено микро обновление базы {0} -> {1}.",
					VersionHelper.VersionToShortString(beforeUpdates),
					VersionHelper.VersionToShortString(currentDB)));
			}
		}

		public void CheckUpdateDB(MySqlConnectionStringBuilder connectionStringBuilder)
		{
			logger.Debug(System.Reflection.Assembly.GetCallingAssembly().FullName);
			Version currentDB = Version.Parse(parametersService.version);
			var appVersion = applicationInfo.Version;
			if(currentDB.Major == appVersion.Major && currentDB.Minor == appVersion.Minor)
				return;

			var update = configuration.Updates.Find(u => u.Source == currentDB);
			if(update != null) {
				if(!permission.HasPermission)
					NotAdminErrorAndExit(false, update.Source, update.Destanation);

				//Увеличиваем время выполнения одной команды до 4 минут. При больших базах процесс обновления может вылетать по таймауту.
				connectionStringBuilder.ConnectionTimeout = 240;

				var dlg = new DBUpdateProcess(
					update,
					new MySQLProvider(
						connectionStringBuilder.GetConnectionString(true),
						new GtkRunOperationService(),
						new GtkQuestionDialogsInteractive()),
						applicationInfo,
						parametersService
					);
				dlg.Show();
				dlg.Run();
				if(!dlg.Success)
					Environment.Exit(1);
				dlg.Destroy();

				parametersService.ReloadParameters();
				if(appVersion.Major != update.Destanation.Major && appVersion.Minor != update.Destanation.Minor)
					CheckUpdateDB(connectionStringBuilder);
			}
			else {
				logger.Error("Версия базы не соответствует программе, но обновление не найдено");
				interactiveMessage.ShowMessage(ImportanceLevel.Error, 
					checkBaseVersion.TextMessage +
					String.Format("\nОбновление базы для версии {0} не поддерживается.", VersionHelper.VersionToShortString(currentDB)));
				Environment.Exit(1);
			}
		}

		public void CheckUpdateDB()
		{
			throw new NotImplementedException();
		}

		private void NotAdminErrorAndExit(bool isMicro, Version from, Version to)
		{
			interactiveMessage.ShowMessage(ImportanceLevel.Error,
				String.Format(
					"Для работы текущей версии программы необходимо провести{0} обновление базы ({1} -> {2}), " +
					"но у вас нет для этого прав. Зайдите в программу под администратором.",
					isMicro ? " микро" : "",
				  	VersionHelper.VersionToShortString(from),
				  	VersionHelper.VersionToShortString(to)
				));
			Environment.Exit(1);
		}
	}
}
