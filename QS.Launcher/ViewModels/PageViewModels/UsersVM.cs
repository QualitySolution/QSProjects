using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Project.Versioning;
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

		private IDbUserManager provider;

		public UsersVM(IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			IServiceProvider serviceProvider) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			var hasSelectedUser = this.WhenAnyValue(x => x.SelectedUser).Select(u => u != null);

			NewUserCommand = ReactiveCommand.Create(StartNewUser);
			EditUserCommand = ReactiveCommand.Create(EditUser);
			DeleteUserCommand = ReactiveCommand.CreateFromTask(DeleteUserAsync, hasSelectedUser);
			RefreshUsersCommand = ReactiveCommand.Create(RefreshUsers);
			BackCommand = ReactiveCommand.Create(() => PopPageCommand?.Execute(null));

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
		private bool isNewUser;
		public bool IsNewUser {
			get => isNewUser;
			set => this.RaiseAndSetIfChanged(ref isNewUser, value);
		}
		public bool CanManageUsers => provider?.CanManageUsers == true;

		public void SetProvider(IDbUserManager userManager) {
			provider = userManager ?? throw new ArgumentNullException(nameof(userManager));

			this.RaisePropertyChanged(nameof(CanManageUsers));
		}

		public void RefreshUsers() {
			Users.Clear();
			SelectedUser = null;
			if(!CanManageUsers)
				return;

			try {
				Users.AddRange(provider.GetUsers());
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось получить список пользователей");
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Управление пользователями");
			}
		}

		private void EditUser() {
			var vm = serviceProvider.GetRequiredService<UserManagementVM>();
			vm.SetContext(provider, SelectedUser, isCreating: false);
			PushPageCommand?.Execute(vm);
		}
		private void StartNewUser() {
			SelectedUser = null;

			var vm = serviceProvider.GetRequiredService<UserManagementVM>();
			vm.SetContext(provider, SelectedUser, isCreating: true);
			PushPageCommand?.Execute(vm);
		}

		private async Task DeleteUserAsync() {
			var user = SelectedUser;
			if(user == null)
				return;
			if(user.IsCurrentUser) {
				interactiveMessage.ShowMessage(ImportanceLevel.Warning, "Нельзя удалить собственного пользователя.", "Управление пользователями");
				return;
			}

			bool confirmed = await Task.Run(() => interactiveQuestion.Question(
				$"Удалить пользователя «{user.Login}»?", "Управление пользователями"));
			if(!confirmed)
				return;

			try {
				await Task.Run(() => provider.DeleteUser(user.Login));
				interactiveMessage.ShowMessage(ImportanceLevel.Success, "Пользователь удалён.", "Управление пользователями");
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось удалить пользователя {0}", user.Login);
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Управление пользователями");
			}
		}
	}
}
