using System;
using System.Threading;
using System.Threading.Tasks;
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
		private readonly IDbScriptsConfiguration scripts;

		public UserCreateDbController(
			INavigationManager navigation,
			IInteractiveService interactive,
			IGuiDispatcher guiDispatcher,
			IDbScriptsConfiguration scripts)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
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

		async void StartCreation(string server, string dbname, string login, string password)
		{
			ParseServer(server, out string host, out uint port);

			bool success = false;
			try {
				var createModel = new MySqlDbCreateModel(
					host, port, login, password,
					scripts,
					Progress,
					interaction: this,
					cancellationToken: CancellationToken.None);

				success = await createModel.RunCreationAsync(dbname, dbTitle: null);
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка создания базы.");
				guiDispatcher.RunInGuiTread(() => interactive.ShowMessage(ImportanceLevel.Error, ex.Message));
			}
			finally {
				guiDispatcher.RunInGuiTread(() => {
					if(progressPage != null)
						navigation.ForceClosePage(progressPage, CloseSource.FromParentPage);
				});
			}

			if(success) {
				guiDispatcher.RunInGuiTread(() =>
					interactive.ShowMessage(ImportanceLevel.Info,
						"Создание базы успешно завершено.\nЗайдите в программу под администратором для добавления пользователей."));
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

		public Task<bool> AskDropExistingDatabaseAsync(string dbName) {
			var tcs = new TaskCompletionSource<bool>();
			guiDispatcher.RunInGuiTread(() => {
				try {
					tcs.SetResult(interactive.Question(
						$"База с именем `{dbName}` уже существует на сервере. Удалить существующую базу перед созданием новой?"));
				}
				catch(Exception ex) { tcs.SetException(ex); }
			});
			return tcs.Task;
		}

		public Task ReportErrorAsync(string text, string lastExecutedStatement) {
			var tcs = new TaskCompletionSource<bool>();
			guiDispatcher.RunInGuiTread(() => {
				try {
					interactive.ShowMessage(ImportanceLevel.Error, text);
					tcs.SetResult(true);
				}
				catch(Exception ex) { tcs.SetException(ex); }
			});
			return tcs.Task;
		}
		#endregion

		#region Свойства процесса

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
