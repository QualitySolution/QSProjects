using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Project.Versioning;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class UserManagementVM : CarouselPageVM {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;
		private readonly IApplicationInfo applicationInfo;

		private IDbUserManager provider;

		public UserManagementVM(
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			IApplicationInfo applicationInfo = null) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.applicationInfo = applicationInfo;

			Users = new ObservableCollection<DbUserInfo>();
			BaseAccesses = new ObservableCollection<BaseAccessRowVM>();

			var canChangeOwnPassword = this.WhenAnyValue(x => x.OwnNewPassword, x => x.OwnConfirmPassword,
				(pass, confirm) => !string.IsNullOrEmpty(pass) && pass == confirm);
			ChangeOwnPasswordCommand = ReactiveCommand.CreateFromTask(ChangeOwnPasswordAsync, canChangeOwnPassword);

			var canSaveUser = this.WhenAnyValue(x => x.EditLogin, x => x.EditNewPassword, x => x.IsNewUser,
				(login, pass, isNew) => !string.IsNullOrWhiteSpace(login) && (!isNew || !string.IsNullOrEmpty(pass)));
			SaveUserCommand = ReactiveCommand.CreateFromTask(SaveUserAsync, canSaveUser);

			var hasSelectedUser = this.WhenAnyValue(x => x.SelectedUser).Select(u => u != null);
			var canSaveAccess = this.WhenAnyValue(x => x.SelectedUser, x => x.CanManageBaseAccess, x => x.BaseAccessLocked,
				(user, canManage, locked) => user != null && canManage && !locked);
			NewUserCommand = ReactiveCommand.Create(StartNewUser);
			DeleteUserCommand = ReactiveCommand.CreateFromTask(DeleteUserAsync, hasSelectedUser);
			SaveAccessCommand = ReactiveCommand.CreateFromTask(SaveAccessAsync, canSaveAccess);
			RefreshUsersCommand = ReactiveCommand.Create(RefreshUsers);
			BackCommand = ReactiveCommand.Create(() => PopPageCommand?.Execute(null));

			this.WhenAnyValue(x => x.SelectedUser)
				.Subscribe(_ => OnSelectedUserChanged());

			this.WhenAnyValue(x => x.SelectedUser, x => x.IsNewUser)
				.Subscribe(_ => {
					this.RaisePropertyChanged(nameof(ShowEditForm));
					this.RaisePropertyChanged(nameof(PasswordWatermark));
				});
		}

		/// <summary>Задаёт провайдера подключения и обновляет состояние страницы.</summary>
		public void SetProvider(IDbUserManager userManager) {
			provider = userManager ?? throw new ArgumentNullException(nameof(userManager));

			OwnNewPassword = null;
			OwnConfirmPassword = null;

			this.RaisePropertyChanged(nameof(CanManageUsers));
			this.RaisePropertyChanged(nameof(CanManageBaseAccess));
			this.RaisePropertyChanged(nameof(ShowName));
			this.RaisePropertyChanged(nameof(ShowEmail));
			this.RaisePropertyChanged(nameof(ShowPhone));
			this.RaisePropertyChanged(nameof(ShowPost));
			this.RaisePropertyChanged(nameof(ShowComment));
			this.RaisePropertyChanged(nameof(ShowAdminFlag));
			this.RaisePropertyChanged(nameof(ShowDisabling));
			this.RaisePropertyChanged(nameof(ShowReadOnly));

			RefreshUsers();
		}

		#region Смена своего пароля

		private string ownNewPassword;
		public string OwnNewPassword {
			get => ownNewPassword;
			set => this.RaiseAndSetIfChanged(ref ownNewPassword, value);
		}

		private string ownConfirmPassword;
		public string OwnConfirmPassword {
			get => ownConfirmPassword;
			set => this.RaiseAndSetIfChanged(ref ownConfirmPassword, value);
		}

		public ReactiveCommand<Unit, Unit> ChangeOwnPasswordCommand { get; }

		private async Task ChangeOwnPasswordAsync() {
			try {
				string newPassword = OwnNewPassword;
				bool ok = await Task.Run(() => provider.ChangeOwnPassword(newPassword));
				if(ok) {
					OwnNewPassword = null;
					OwnConfirmPassword = null;
					interactiveMessage.ShowMessage(ImportanceLevel.Success, "Пароль изменён.", "Смена пароля");
				}
				else
					interactiveMessage.ShowMessage(ImportanceLevel.Error, "Не удалось изменить пароль.", "Смена пароля");
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось сменить собственный пароль");
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Смена пароля");
			}
		}

		#endregion

		#region Управление пользователями

		public bool CanManageUsers => provider?.CanManageUsers == true;

		/// <summary>Может ли текущий пользователь сохранять доступы к базам</summary>
		public bool CanManageBaseAccess => provider?.CanManageBaseAccess == true;

		public bool ShowName => (provider?.SupportedUserFields.HasFlag(DbUserFields.Name)) == true;
		public bool ShowEmail => (provider?.SupportedUserFields.HasFlag(DbUserFields.Email)) == true;
		public bool ShowPhone => (provider?.SupportedUserFields.HasFlag(DbUserFields.Phone)) == true;
		public bool ShowPost => (provider?.SupportedUserFields.HasFlag(DbUserFields.Post)) == true;
		public bool ShowComment => (provider?.SupportedUserFields.HasFlag(DbUserFields.Comment)) == true;
		public bool ShowAdminFlag => (provider?.SupportedUserFields.HasFlag(DbUserFields.AdminFlag)) == true;
		public bool ShowDisabling => (provider?.SupportedUserFields.HasFlag(DbUserFields.Disabling)) == true;
		public bool ShowReadOnly => (provider?.SupportedUserFields.HasFlag(DbUserFields.BaseReadOnly)) == true;

		public ObservableCollection<DbUserInfo> Users { get; }

		private DbUserInfo selectedUser;
		public DbUserInfo SelectedUser {
			get => selectedUser;
			set => this.RaiseAndSetIfChanged(ref selectedUser, value);
		}

		public bool HasSelectedUser => SelectedUser != null;

		/// <summary>Форма редактирования видна, только когда выбран пользователь или создаётся новый</summary>
		public bool ShowEditForm => IsNewUser || SelectedUser != null;

		/// <summary>Подсказка в поле пароля зависит от режима формы</summary>
		public string PasswordWatermark => IsNewUser
			? "задайте пароль нового пользователя"
			: "оставьте пустым, чтобы не менять";

		public ReactiveCommand<Unit, Unit> NewUserCommand { get; }
		public ReactiveCommand<Unit, Unit> SaveUserCommand { get; }
		public ReactiveCommand<Unit, Unit> DeleteUserCommand { get; }
		public ReactiveCommand<Unit, Unit> SaveAccessCommand { get; }
		public ReactiveCommand<Unit, Unit> RefreshUsersCommand { get; }
		public ReactiveCommand<Unit, Unit> BackCommand { get; }

		public void RefreshUsers() {
			Users.Clear();
			SelectedUser = null;
			if(!CanManageUsers)
				return;

			try {
				foreach(var user in provider.GetUsers())
					Users.Add(user);
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось получить список пользователей");
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Управление пользователями");
			}
		}

		private void OnSelectedUserChanged() {
			this.RaisePropertyChanged(nameof(HasSelectedUser));
			BaseAccesses.Clear();
			this.RaisePropertyChanged(nameof(BaseAccessLocked));
			if(SelectedUser == null) {
				IsNewUser = false;
				return;
			}
			LoadEditBuffer(SelectedUser);
			IsNewUser = false;
			LoadBaseAccess(SelectedUser.Login);
		}

		private void StartNewUser() {
			SelectedUser = null;
			ClearEditBuffer();
			IsNewUser = true;
		}

		private async Task SaveUserAsync() {
			var user = new DbUserInfo {
				Login = EditLogin,
				Name = EditName,
				Email = EditEmail,
				Phone = EditPhone,
				Post = EditPost,
				Comment = EditComment,
				Disabled = EditDisabled,
				IsAdmin = EditIsAdmin
			};
			bool creating = IsNewUser;
			string password = EditNewPassword;

			try {
				await Task.Run(() => {
					if(creating)
						provider.CreateUser(user, password);
					else
						provider.UpdateUser(user, password);
				});
				interactiveMessage.ShowMessage(ImportanceLevel.Success,
					creating ? "Пользователь создан." : "Изменения сохранены.", "Управление пользователями");
				RefreshUsers();
				SelectedUser = Users.FirstOrDefault(u => string.Equals(u.Login, user.Login, StringComparison.OrdinalIgnoreCase));
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось сохранить пользователя {0}", user.Login);
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Управление пользователями");
			}
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
				RefreshUsers();
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось удалить пользователя {0}", user.Login);
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Управление пользователями");
			}
		}

		#endregion

		#region Редактируемые поля пользователя

		private bool isNewUser;
		public bool IsNewUser {
			get => isNewUser;
			set => this.RaiseAndSetIfChanged(ref isNewUser, value);
		}

		private string editLogin;
		public string EditLogin {
			get => editLogin;
			set => this.RaiseAndSetIfChanged(ref editLogin, value);
		}

		private string editName;
		public string EditName {
			get => editName;
			set => this.RaiseAndSetIfChanged(ref editName, value);
		}

		private string editEmail;
		public string EditEmail {
			get => editEmail;
			set => this.RaiseAndSetIfChanged(ref editEmail, value);
		}

		private string editPhone;
		public string EditPhone {
			get => editPhone;
			set => this.RaiseAndSetIfChanged(ref editPhone, value);
		}

		private string editPost;
		public string EditPost {
			get => editPost;
			set => this.RaiseAndSetIfChanged(ref editPost, value);
		}

		private string editComment;
		public string EditComment {
			get => editComment;
			set => this.RaiseAndSetIfChanged(ref editComment, value);
		}

		private bool editDisabled;
		public bool EditDisabled {
			get => editDisabled;
			set => this.RaiseAndSetIfChanged(ref editDisabled, value);
		}

		private bool editIsAdmin;
		public bool EditIsAdmin {
			get => editIsAdmin;
			set => this.RaiseAndSetIfChanged(ref editIsAdmin, value);
		}

		private string editNewPassword;
		public string EditNewPassword {
			get => editNewPassword;
			set => this.RaiseAndSetIfChanged(ref editNewPassword, value);
		}

		private void LoadEditBuffer(DbUserInfo user) {
			EditLogin = user.Login;
			EditName = user.Name;
			EditEmail = user.Email;
			EditPhone = user.Phone;
			EditPost = user.Post;
			EditComment = user.Comment;
			EditDisabled = user.Disabled;
			EditIsAdmin = user.IsAdmin;
			EditNewPassword = null;
		}

		private void ClearEditBuffer() {
			EditLogin = null;
			EditName = null;
			EditEmail = null;
			EditPhone = null;
			EditPost = null;
			EditComment = null;
			EditDisabled = false;
			EditIsAdmin = false;
			EditNewPassword = null;
		}

		#endregion

		#region Доступ к базам

		public ObservableCollection<BaseAccessRowVM> BaseAccesses { get; }

		// Доступ пользователя следует из глобальных прав на сервер и точечно не настраивается
		public bool BaseAccessLocked => BaseAccesses.Count > 0 && BaseAccesses.All(r => !r.CanEdit);

		private void LoadBaseAccess(string login) {
			if(applicationInfo == null)
				return;
			try {
				var rows = provider.GetUserBaseAccess(login, applicationInfo);
				foreach(var row in rows)
					BaseAccesses.Add(new BaseAccessRowVM(row, ShowReadOnly));

				var profile = rows.FirstOrDefault(r => !string.IsNullOrEmpty(r.Name) || !string.IsNullOrEmpty(r.Email));
				if(profile != null) {
					if(string.IsNullOrEmpty(EditName))
						EditName = profile.Name;
					if(string.IsNullOrEmpty(EditEmail))
						EditEmail = profile.Email;
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось получить доступы пользователя {0}", login);
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Доступ к базам");
			}
			this.RaisePropertyChanged(nameof(BaseAccessLocked));
		}

		private async Task SaveAccessAsync() {
			var user = SelectedUser;
			if(user == null)
				return;
			// Сохраняем только изменённые строки - каждая строка это отдельный запрос к серверу
			var changedRows = BaseAccesses.Where(r => r.IsDirty).ToList();
			if(changedRows.Count == 0)
				return;
			try {
				string name = EditName;
				string email = EditEmail;
				await Task.Run(() => {
					foreach(var row in changedRows) {
						var access = row.ToAccess();
						// профиль пишется в таблицу users каждой базы, куда выдаём доступ
						access.Name = name;
						access.Email = email;
						provider.SetUserBaseAccess(user.Login, access, applicationInfo);
					}
				});
				foreach(var row in changedRows)
					row.AcceptChanges();
				interactiveMessage.ShowMessage(ImportanceLevel.Success, "Доступы сохранены", "Доступ к базам");
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось сохранить доступы пользователя {0}", user.Login);
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Доступ к базам");
			}
		}

		#endregion
	}
}
