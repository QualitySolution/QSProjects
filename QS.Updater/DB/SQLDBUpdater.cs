using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using Autofac;
using MySql.Data.MySqlClient;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Versioning;
using QS.Services;
using QS.Updater.DB.ViewModels;
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
		private readonly INavigationManager navigation;
		private readonly IGuiDispatcher gui;
		private readonly dynamic parametersService;
		private readonly DbConnection connection;
		private readonly MySqlConnectionStringBuilder connectionStringBuilder;
		private readonly IUserService userService;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IInteractiveMessage interactiveMessage;

		public SQLDBUpdater(UpdateConfiguration configuration, INavigationManager navigation, IGuiDispatcher gui, BaseParameters.ParametersService parametersService, IApplicationInfo applicationInfo, DbConnection connection, MySqlConnectionStringBuilder connectionStringBuilder, IUserService userService, IUnitOfWorkFactory unitOfWorkFactory, IInteractiveMessage interactiveMessage)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.gui = gui ?? throw new ArgumentNullException(nameof(gui));
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		#region Private

		private Version CurrentDBVersion => parametersService.micro_updates != null ? parametersService.micro_updates(typeof(Version)) : parametersService.version(typeof(Version));

		public bool HasUpdates => configuration.GetHopsToLast(CurrentDBVersion).Any();

		#endregion
		private void RunMicroUpdateOnly()
		{
			var currentDB = CurrentDBVersion;
			var beforeUpdates = currentDB;
			var hops = configuration.GetHopsToLast(currentDB).ToList();
			logger.Info("Начинаем микро обновление(текущая версия:{0})", VersionHelper.VersionToShortString(currentDB));

			foreach(var update in hops) {
				logger.Info("Обновляемся до {0}", VersionHelper.VersionToShortString(update.Destination));
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
					currentDB = update.Destination;
				}
				catch(Exception ex) {
					trans.Rollback();
					throw ex;
				}
				parametersService.micro_updates = VersionHelper.VersionToShortString(currentDB);
			}
				
			interactiveMessage.ShowMessage(ImportanceLevel.Info, String.Format("Выполнено микро обновление базы {0} -> {1}.",
				VersionHelper.VersionToShortString(beforeUpdates),
				VersionHelper.VersionToShortString(currentDB)));
		}

		private void RunUpdateDB()
		{
			//Увеличиваем время выполнения одной команды до 4 минут. При больших базах процесс обновления может вылетать по таймауту.
			connectionStringBuilder.ConnectionTimeout = 240;

			var page = navigation.OpenViewModel<UpdateProcessViewModel>(null, addingRegistrations: c => 
				c.Register(cxt => connectionStringBuilder));
			var isClosed = false;
			CloseSource source= CloseSource.Self;
			page.PageClosed += (sender, e) => {
				isClosed = true;
				source = e.CloseSource;
			};
			gui.WaitInMainLoop(() => isClosed);
			if(source != CloseSource.Save) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "База данных не обновлена, дальнейшая работа приложения не возможна. Приложение будет закрыто.", "Выход");
				Environment.Exit(1);
			}
		}

		public void UpdateDB()
		{
			var hops = configuration.GetHopsToLast(CurrentDBVersion).ToList();
			if(!hops.Any()) {
				logger.Info("Нет обновлений для базы данных.");
				return;
			}

			using(var uow = unitOfWorkFactory.CreateWithoutRoot()) {
				if(connectionStringBuilder.UserID != "root" && !userService.GetCurrentUser(uow).IsAdmin)
					NotAdminErrorAndExit(CurrentDBVersion, hops.Last().Destination);
			}

			if(hops.All(x => x.UpdateType == UpdateType.MicroUpdate))
				RunMicroUpdateOnly();
			else
				RunUpdateDB();
		}

		private void NotAdminErrorAndExit(Version from, Version to)
		{
			interactiveMessage.ShowMessage(ImportanceLevel.Error,
				String.Format(
					"Для работы текущей версии программы необходимо провести обновление базы ({0} -> {1}), " +
					"но у вас нет для этого прав. Зайдите в программу под администратором.",
				  	VersionHelper.VersionToShortString(from),
				  	VersionHelper.VersionToShortString(to)
				));
			Environment.Exit(1);
		}
	}
}
