using System;
using System.Threading;
using QS.Project.VersionControl;
using QSUpdater;

namespace QS.Updater
{
	public class ApplicationUpdater
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public UpdateResult UpdateResult { get; private set; }

		private readonly IUpdateService updateService;
		private readonly IApplicationInfo application;
		private readonly ISkipVersionState skip;
		private readonly IUpdaterUI uI;

		public ApplicationUpdater(IUpdaterUI updaterUI, IUpdateService updateService, IApplicationInfo application, ISkipVersionState skips)
		{
			this.updateService = updateService;
			this.application = application;
			this.skip = skips;
			this.uI = updaterUI;
		}

		public void StartCheckUpdateThread (UpdaterFlags flags)
		{
			if (flags.HasFlag(UpdaterFlags.StartInThread)) {
				Thread loadThread = new Thread (() => ThreadWorks (flags));
				loadThread.Start ();
				if (flags.HasFlag(UpdaterFlags.UpdateRequired))
					loadThread.Join ();
			} else
				ThreadWorks (flags);
		}

		void ThreadWorks (UpdaterFlags flags)
		{
			string checkVersion = String.Empty, checkResult = String.Empty;
			try {
				logger.Info ("Получаем данные от сервера");
				string parameters = String.Format ("product.{0};edition.{1};serial.{2};major.{3};minor.{4};build.{5};revision.{6}",
												application.ProductName,
				                                application.Edition,
												application.SerialNumber,
												application.Version.Major,
												application.Version.Minor,
												application.Version.Build,
												application.Version.Revision); 

				var updateResult = updateService.checkForUpdate (parameters);
				if (flags.HasFlag(UpdaterFlags.ShowAnyway) || (updateResult.HasUpdate && !skip.IsSkipedVersion(updateResult.NewVersion)))
				{
					uI.ShowAppNewVersionDialog(updateResult, flags);
				}
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка доступа к серверу обновления.");
				if (flags.HasFlag(UpdaterFlags.ShowAnyway))
					uI.InteractiveMessage.ShowMessage(Dialog.ImportanceLevel.Error, "Не удалось подключиться к серверу обновлений.\nПожалуйста, повторите попытку позже.");
				if (flags.HasFlag(UpdaterFlags.UpdateRequired))
					Environment.Exit (1);
			}
		}
	}
}