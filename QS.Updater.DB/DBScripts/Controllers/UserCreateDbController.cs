using System;
using System.Threading;
using QS.DBScripts.Models;
using QS.DBScripts.ViewModels;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.Navigation;

namespace QS.DBScripts.Controllers
{
	public class UserCreateDbController : IDBCreator, IDbCreatorInteraction
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly INavigationManager navigation;
		private readonly IInteractiveService interactive;
		private readonly IGuiDispatcher guiDispatcher;
		private readonly CreationScript creationScript;

		public UserCreateDbController(
			INavigationManager navigation,
			IInteractiveService interactive,
			IGuiDispatcher guiDispatcher,
			CreationScript creationScript)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.creationScript = creationScript ?? throw new ArgumentNullException(nameof(creationScript));
		}

		public void RunCreation(string server, string dbname)
		{
			var page = navigation.OpenViewModel<AdminLoginViewModel, string>(null, "root");
			page.PageClosed += delegate (object sender, PageClosedEventArgs e) {
				if(e.CloseSource == CloseSource.Save) {
					//Вызываем через guiDispatcher для того чтобы диалог с паролем закрылся, до начала операции создания БД.
					guiDispatcher.RunInGuiTread(() => StartCreation(server, dbname, page.ViewModel.Login, page.ViewModel.Password));
				}
			};
		}

		void StartCreation(string server, string dbname, string login, string password)
		{
			try {
				ParseServer(server, out string host, out uint port);

				var createModel = new MySqlDbCreateModel(
					host, port, login, password,
					creationScript,
					Progress,
					interaction: this,
					cancellationToken: CancellationToken.None);

				bool success = createModel.RunCreation(dbname);
				if(success)
					interactive.ShowMessage(ImportanceLevel.Info, "Создание базы успешно завершено.\nЗайдите в программу под администратором для добавления пользователей.");
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка создания базы.");
				interactive.ShowMessage(ImportanceLevel.Error, ex.Message);
			}
			finally {
				if(progressPage != null)
					navigation.ForceClosePage(progressPage, CloseSource.FromParentPage);
			}
		}

		private static void ParseServer(string server, out string host, out uint port) {
			port = 3306;
			var parts = (server ?? string.Empty).Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
			if(parts.Length == 0)
				throw new InvalidOperationException("Имя сервера не корректно.");
			host = parts[0];
			if(parts.Length > 1)
				uint.TryParse(parts[1], out port);
		}

		#region IDbCreatorInteraction
		//Создание идет в GUI-потоке (как и раньше), прогресс сам прокачивает событийный цикл,
		//поэтому диалоги показываем напрямую.

		public bool AskDropExistingDatabase(string dbName)
		{
			return interactive.Question($"База с именем `{dbName}` уже существует на сервере. Удалить существующую базу перед созданием новой?");
		}

		public void ReportError(string text, string lastExecutedStatement)
		{
			interactive.ShowMessage(ImportanceLevel.Error, text);
		}
		#endregion

		#region Progress page

		IPage<ProgressWindowViewModel> progressPage;
		public IProgressBarDisplayable Progress {
			get {
				if(progressPage == null) {
					progressPage = navigation.OpenViewModel<ProgressWindowViewModel>(null);
					progressPage.PageClosed += (sender, e) => progressPage = null;
					progressPage.ViewModel.Title = "Создание базы данных";
				}
				return progressPage.ViewModel.Progress;
			}
		}
		#endregion
	}
}
