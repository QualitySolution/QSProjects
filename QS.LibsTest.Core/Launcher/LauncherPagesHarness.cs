using NSubstitute;
using QS.DbManagement;
using QS.DbManagement.Creation;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Dialog;
using QS.Launcher.AppRunner;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using QS.ErrorReporting;
using QS.Project.Versioning;
using QS.Testing.Gui;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace QS.Launcher.Test {
	/// <summary>
	/// Сборка страниц лаунчера
	/// </summary>
	public class LauncherPagesHarness {
		private readonly ConnectionTypeBase connectionType;
		private readonly string connectionTitle;

		public LauncherPagesHarness(ConnectionTypeBase connectionType, string connectionTitle, byte productCode) {
			this.connectionType = connectionType ?? throw new ArgumentNullException(nameof(connectionType));
			this.connectionTitle = connectionTitle;

			InteractiveMessage = Substitute.For<IInteractiveMessage>();
			InteractiveQuestion = Substitute.For<IInteractiveQuestion>();
			AppRunner = Substitute.For<IAppRunner>();

			ApplicationInfo = Substitute.For<IApplicationInfo>();
			ApplicationInfo.ProductCode.Returns(productCode);

			ScriptsConfiguration = Substitute.For<IDbScriptsConfiguration>();
			ScriptsConfiguration.HasCreationScript().Returns(true);

			// Скрипт создания и модели наполнения приносит приложение, а не файлы рядом с тестами:
			// странице важно только то, что они настроены
			CreationMap = new DbResourcesCreationMap();
			CreationMap.Register(typeof(EmbeddedCreationResources), typeof(NoOpCreationModel));
			CreationMap.Register(typeof(DbDumpResources), typeof(NoOpCreationModel));

			Options = new LauncherOptions { AppTitle = "Тест", IsStandalone = true };

			ErrorHandling = new ErrorHandlingService(
				new List<IErrorHandler>(), InteractiveMessage, InteractiveQuestion);

			Navigation = new LauncherNavigation();
			Navigation.Pages.CollectionChanged += (sender, e) => {
				if(e.Action == NotifyCollectionChangedAction.Remove)
					PopCount += e.OldItems.Count;
			};

			ServiceProvider = Substitute.For<IServiceProvider>();
			ServiceProvider.GetService(typeof(UsersVM)).Returns(_ => BuildUsersVM());
			ServiceProvider.GetService(typeof(UserManagementVM)).Returns(_ => BuildUserManagementVM());
			ServiceProvider.GetService(typeof(CreateDataBaseProgressVM)).Returns(_ => BuildProgressVM());
		}

		public IInteractiveMessage InteractiveMessage { get; }
		public IInteractiveQuestion InteractiveQuestion { get; }
		public IAppRunner AppRunner { get; }
		public IApplicationInfo ApplicationInfo { get; }
		public IDbScriptsConfiguration ScriptsConfiguration { get; }

		/// <summary>Модели наполнения, известные приложению; null - оно не зарегистрировало ни одной</summary>
		public DbResourcesCreationMap CreationMap { get; set; }
		public IServiceProvider ServiceProvider { get; }
		public LauncherOptions Options { get; }
		public IErrorHandlingService ErrorHandling { get; }

		public LauncherNavigation Navigation { get; }

		public List<object> PushedPages => Navigation.Pages.Skip(1).Cast<object>().ToList();

		public int PopCount { get; private set; }

		public UserManagementVM LastForm => (UserManagementVM)PushedPages.Last();
		
		public async Task<UserManagementVM> LastFormLoaded() {
			var form = LastForm;
			await form.LoadSelectedUserCommand.IsExecuting.Where(executing => !executing).FirstAsync();
			return form;
		}

		public static void UseImmediateSchedulers() {
			// В тестах нет диспетчера интерфейса
			// команды и подписки должны выполняться сразу, иначе await над ReactiveCommand не дождётся результата
			RxApp.MainThreadScheduler = ImmediateScheduler.Instance;
			RxApp.TaskpoolScheduler = ImmediateScheduler.Instance;
		}

		/// <summary>да на любой вопрос пользователю</summary>
		public void AnswerYesToQuestions() =>
			InteractiveQuestion.Question(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

		/// <summary>нет на любой вопрос пользователю</summary>
		public void AnswerNoToQuestions() =>
			InteractiveQuestion.Question(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

		public DataBasesVM BuildDataBasesVM() =>
			AsRootIfFirst(new DataBasesVM(AppRunner, Navigation, InteractiveMessage, InteractiveQuestion,
				Options, ServiceProvider, new DbCapabilities(ScriptsConfiguration, CreationMap), ErrorHandling));

		public UsersVM BuildUsersVM() =>
			AsRootIfFirst(new UsersVM(Navigation, InteractiveMessage, InteractiveQuestion, ServiceProvider, ErrorHandling));

		public UserManagementVM BuildUserManagementVM() =>
			AsRootIfFirst(new UserManagementVM(Navigation, InteractiveMessage, InteractiveQuestion, ErrorHandling));

		public CreateDataBaseProgressVM BuildProgressVM() =>
			AsRootIfFirst(new CreateDataBaseProgressVM(Navigation, new GuiDispatcherForTests(), ServiceProvider, ErrorHandling));

		/// <summary>Прогоняет открытую страницу прогресса</summary>
		public async Task<CreateDataBaseProgressVM> RunLastProgressPage() {
			var progress = (CreateDataBaseProgressVM)PushedPages.Last();
			await progress.RunAsync();
			return progress;
		}

		/// <summary>спискок баз когда пользователь только что вошёл</summary>
		public async Task<DataBasesVM> OpenDatabasesPage(IDbProvider provider) {
			var vm = BuildDataBasesVM();
			var connection = new Connection(connectionType,
				new Dictionary<string, string> { { "Title", connectionTitle } });
			await vm.SetProviderAsync(provider, connection, () => { });
			return vm;
		}

		/// <summary>спискок пользователей когда открыли управление пользователями</summary>
		public async Task<UsersVM> OpenUsersPage(IDbUserManager provider) {
			var vm = BuildUsersVM();
			vm.SetProvider(provider);

			// SetProvider запускает Execute().Subscribe() без ожидания
			// Дожидаемся её и перечитываем список
			await vm.RefreshUsersCommand.IsExecuting.Where(executing => !executing).FirstAsync();
			await vm.RefreshUsersCommand.Execute();
			return vm;
		}

		/// <summary>
		/// Наполнения в тестах страницы не происходит - карте важно лишь то, что модель зарегистрирована
		/// </summary>
		private sealed class NoOpCreationModel : IDbCreatorModel {
			private readonly DbCreationResources resources;
			public NoOpCreationModel(DbCreationResources resources) => this.resources = resources;

			public bool RunCreation(string dbName, string dbTitle) => resources != null;
		}

		private T AsRootIfFirst<T>(T vm) where T : CarouselPageVM {
			if(Navigation.Pages.Count == 0)
				Navigation.SetRoots(vm);
			return vm;
		}
	}
}
