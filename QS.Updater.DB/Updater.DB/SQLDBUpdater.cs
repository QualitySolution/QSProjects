using System;
using System.Linq;
using Autofac;
using MySql.Data.MySqlClient;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
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
		private readonly MySqlConnectionStringBuilder connectionStringBuilder;
		private readonly IUserService userService;
		private readonly IInteractiveMessage interactiveMessage;

		public SQLDBUpdater(UpdateConfiguration configuration, INavigationManager navigation, IGuiDispatcher gui, BaseParameters.ParametersService parametersService, MySqlConnectionStringBuilder connectionStringBuilder, IUserService userService, IInteractiveMessage interactiveMessage)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.gui = gui ?? throw new ArgumentNullException(nameof(gui));
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
			this.connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool HasUpdates => configuration.GetHopsToLast(CurrentDBVersion).Any();
		
		#region Private

		//FIXME Здесь проверка micro_updates оставлена для совместимости и возможности корректного обновления со старых версий программ. Удаление сделает невозможным начать обновление с установленного микроапдейта.
		private Version CurrentDBVersion => parametersService.micro_updates != null ? parametersService.micro_updates(typeof(Version)) : parametersService.version(typeof(Version));
		
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
		#endregion
		
		public void UpdateDB()
		{
			var hops = configuration.GetHopsToLast(CurrentDBVersion).ToList();
			if(!hops.Any()) {
				logger.Info("Нет обновлений для базы данных.");
				return;
			}

			if (parametersService.UpdateInProgress(typeof(bool?)) ?? false) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "Предыдущее обновления базы данных не было завершено. " +
				                                                      "База данных поломана. Восстановите базу данных из резервной копии и попробуйте снова. " +
				                                                      "Если проблема повторяется, напишите в тех поддержку: support@qsolution.ru");
				Environment.Exit(1);
			}
			
			if(connectionStringBuilder.UserID != "root" && !userService.GetCurrentUser().IsAdmin)
				NotAdminErrorAndExit(CurrentDBVersion, hops.Last().Destination);

			RunUpdateDB();
		}

		private void NotAdminErrorAndExit(Version from, Version to)
		{
			interactiveMessage.ShowMessage(ImportanceLevel.Error,
				String.Format(
					"Для работы текущей версии программы необходимо провести обновление базы ({0} -> {1}), " +
					"но у вас нет для этого прав. Зайдите в программу с правами администратора.",
				  	VersionHelper.VersionToShortString(from),
				  	VersionHelper.VersionToShortString(to)
				));
			Environment.Exit(1);
		}
	}
}
