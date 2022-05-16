using System;
using System.Threading.Tasks;
using Autofac;
using QS.BaseParameters;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class ApplicationUpdater
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static Uri ServiceUrl = new Uri("http://saas.qsolution.ru:2048/Updater");

		public ApplicationUpdater()
		{
		}

		public void StartCheckUpdate(UpdaterFlags flags, ILifetimeScope autofacScope)
		{
			ThreadWorks(flags, autofacScope);
		}

		/// <summary>
		/// Запускает процесс обновления в отдельном потоке.
		/// </summary>
		/// <param name="flags">Флаги апдейтера</param>
		/// <param name="container">Контейнер Autofac нужен для создания скопа</param>
		public void StartCheckUpdateThread (UpdaterFlags flags, ILifetimeScope container)
		{
			//Важно здесь создаем новый скоп, так как общий снаружи может закрыться.
			var threadScope = container.BeginLifetimeScope();
			Task.Run(() => ThreadWorks(flags, threadScope))
				.ContinueWith((tsk) => {
					if(tsk.IsFaulted)
						logger.Error(tsk.Exception, "Ошибка при выполении запроса обновления в фоновом потоке.");
					threadScope.Dispose();
					logger.Debug("Скоп потока убит.");
				});
		}

		void ThreadWorks (UpdaterFlags flags, ILifetimeScope threadScope)
		{
			string checkVersion = String.Empty, checkResult = String.Empty;
			var application = threadScope.Resolve<IApplicationInfo>();
			dynamic parametersService = ResolutionExtensions.ResolveOptional<ParametersService>(threadScope);
			var updateService = threadScope.Resolve<IUpdateService>();
			var skip = threadScope.Resolve<ISkipVersionState>();
			var uI = threadScope.Resolve<IUpdaterUI>();
			try {
				logger.Info("Запрашиваем информацию о новых версиях с сервера");
				string parameters = String.Format("product.{0};edition.{1};serial.{2};major.{3};minor.{4};build.{5};revision.{6}",
												application.ProductName,
												application.Modification,
												parametersService?.serial_number,
												application.Version.Major,
												application.Version.Minor,
												application.Version.Build,
												application.Version.Revision);

				var updateResult = updateService.checkForUpdate(parameters);
				if(flags.HasFlag(UpdaterFlags.ShowAnyway) || (updateResult.HasUpdate && !skip.IsSkipedVersion(updateResult.NewVersion))) {
					uI.ShowAppNewVersionDialog(updateResult, flags);
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка доступа к серверу обновления.");
				if(flags.HasFlag(UpdaterFlags.ShowAnyway))
					uI.InteractiveMessage.ShowMessage(Dialog.ImportanceLevel.Error, "Не удалось подключиться к серверу обновлений.\nПожалуйста, повторите попытку позже.");
				if(flags.HasFlag(UpdaterFlags.UpdateRequired))
					Environment.Exit(1);
			}
		}
	}
}