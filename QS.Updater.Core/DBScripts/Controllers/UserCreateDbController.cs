using System;
using Autofac;
using QS.DBScripts.Models;
using QS.DBScripts.ViewModels;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.Navigation;

namespace QS.DBScripts.Controllers
{
	public class UserCreateDbController : IDBCreator, IDbCreateController
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly INavigationManager navigation;
		private readonly ILifetimeScope autofacScope;
		private readonly IInteractiveService interactive;
		private readonly IGuiDispatcher guiDispatcher;

		public UserCreateDbController(INavigationManager navigation, ILifetimeScope autofacScope, IInteractiveService interactive, IGuiDispatcher guiDispatcher)
		{

			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
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
			var createMOdel = autofacScope.Resolve<MySqlDbCreateModel>(new TypedParameter(typeof(IDbCreateController), this));
			try {
				bool success = createMOdel.RunCreation(server, dbname, login, password);
				if (success)
					interactive.ShowMessage(ImportanceLevel.Info, "Создание базы успешно завершено.\nЗайдите в программу под администратором для добавления пользователей.");
			}
			finally {
				if(progressPage != null)
					navigation.ForceClosePage(progressPage, CloseSource.FromParentPage);
			}
		}

		#region Взаимодействие с моделью
		public void WasError(string text)
		{
			interactive.ShowMessage(ImportanceLevel.Error, text);
		}

		public bool BaseExistDropIt(string dbname)
		{
			return interactive.Question($"База с именем `{dbname}` уже существует на сервере. Удалить существующую базу перед соданием новой?");
		}

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
		#endregion
	}
}