using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.ErrorReporting;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class UsersVM : CarouselPageVM
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;
		private readonly IServiceProvider serviceProvider;
		private readonly IErrorHandlingService errorHandling;

		private IDbUserManager provider;

		public UsersVM(LauncherNavigation navigation,
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			IServiceProvider serviceProvider,
			IErrorHandlingService errorHandling) : base(navigation) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			this.errorHandling = errorHandling ?? throw new ArgumentNullException(nameof(errorHandling));

			var hasSelectedUser = this.WhenAnyValue(x => x.SelectedUser).Select(u => u != null);

			NewUserCommand = ReactiveCommand.Create(StartNewUser);
			EditUserCommand = ReactiveCommand.Create(EditUser, hasSelectedUser);
			DeleteUserCommand = ReactiveCommand.CreateFromTask(DeleteUserAsync, hasSelectedUser);
			RefreshUsersCommand = ReactiveCommand.CreateFromTask(
				() => RunBusyAsync("Загрузка списка пользователей", RefreshUsers));
			BackCommand = ReactiveCommand.Create(Navigation.Pop);

			Users = new ObservableCollection<DbUserInfo>();
		}

		public ReactiveCommand<Unit, Unit> RefreshUsersCommand { get; }
		public ReactiveCommand<Unit, Unit> BackCommand { get; }
		public ReactiveCommand<Unit, Unit> NewUserCommand { get; }
		public ReactiveCommand<Unit, Unit> EditUserCommand { get; }
		public ReactiveCommand<Unit, Unit> DeleteUserCommand { get; }


		public ObservableCollection<DbUserInfo> Users { get; }
		private DbUserInfo selectedUser;
		public DbUserInfo SelectedUser {
			get => selectedUser;
			set {
				this.RaiseAndSetIfChanged(ref selectedUser, value);
			}
		}

		public bool HasSelectedUser => SelectedUser != null;
		public bool CanManageUsers => provider?.CanManageUsers == true;

		public void SetProvider(IDbUserManager userManager) {
			provider = userManager ?? throw new ArgumentNullException(nameof(userManager));

			this.RaisePropertyChanged(nameof(CanManageUsers));
			RefreshUsersCommand.Execute().Subscribe();
		}

		public async Task RefreshUsers() {
			Users.Clear();
			SelectedUser = null;
			if(!CanManageUsers)
				return;

			try {
				var users = await Task.Run(() => provider.GetUsers());
				Users.AddRange(users);
			}
			catch(Exception ex) {
				errorHandling.Handle(ex, "Управление пользователями");
			}
		}

		private void EditUser() {
			OpenEditor(SelectedUser, isCreating: false);
		}

		private void StartNewUser() {
			SelectedUser = null;
			OpenEditor(null, isCreating: true);
		}

		private void OpenEditor(DbUserInfo user, bool isCreating) {
			var vm = serviceProvider.GetRequiredService<UserManagementVM>();
			vm.SetContext(provider, user, isCreating);
			vm.OperationCompleted += () => RefreshUsersCommand.Execute().Subscribe();
			Navigation.Push(vm);
		}

		private async Task DeleteUserAsync() {
			var user = SelectedUser;
			if(user == null)
				return;
			if(user.IsCurrentUser) {
				interactiveMessage.ShowMessage(ImportanceLevel.Warning, "Нельзя удалить собственного пользователя.", "Управление пользователями");
				return;
			}

			bool confirmed = await interactiveQuestion.AskInBackground(
				$"Удалить пользователя «{user.Login}»?", "Управление пользователями");
			if(!confirmed)
				return;

			await RunBusyAsync("Удаление пользователя", async () => {
				try {
					await Task.Run(() => provider.DeleteUser(user.Login));
					interactiveMessage.ShowMessage(ImportanceLevel.Success, "Пользователь удалён.", "Управление пользователями");
					await RefreshUsers();
				}
				catch(Exception ex) {
					errorHandling.Handle(ex, "Управление пользователями");
				}
			});
		}
	}
}
