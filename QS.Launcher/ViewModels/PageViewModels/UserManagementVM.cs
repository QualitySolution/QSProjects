using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Project.Versioning;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class UserManagementVM : CarouselPageVM {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;
		private readonly IApplicationInfo applicationInfo;

		private readonly string messageTitle = "Управление пользователями";

		private IDbUserManager provider;

		public UserManagementVM(
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			IApplicationInfo applicationInfo = null) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.applicationInfo = applicationInfo;

			BaseAccesses = new ObservableCollection<BaseAccessRowVM>();

			var canSaveUser = this.WhenAnyValue(x => x.EditLogin, x => x.EditNewPassword, x => x.IsNewUser,
				(login, pass, isNew) => !string.IsNullOrWhiteSpace(login) && (!isNew || !string.IsNullOrEmpty(pass)));
			SaveCommand = ReactiveCommand.CreateFromTask(Save, canSaveUser);

			BackCommand = ReactiveCommand.CreateFromTask(GoBack);

			this.WhenAnyValue(x => x.SelectedUser)
				.Subscribe(_ => OnSelectedUserChanged());

			this.WhenAnyValue(x => x.SelectedUser, x => x.IsNewUser)
				.Subscribe(_ => {
					this.RaisePropertyChanged(nameof(ShowEditForm));
					this.RaisePropertyChanged(nameof(PasswordWatermark));
				});
		}

		public void SetContext(IDbUserManager userManager, DbUserInfo user, bool isCreating) {
			provider = userManager ?? throw new ArgumentNullException(nameof(userManager));

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
			this.RaisePropertyChanged(nameof(ShowAppPermissions));

			SelectedUser = null;
			if(isCreating) {
				ClearEditBuffer();
				IsNewUser = true;
			}
			else
				SelectedUser = user;
		}

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
		public bool ShowAppPermissions => (provider?.SupportedUserFields.HasFlag(DbUserFields.BaseAppPermissions)) == true;

		private DbUserInfo selectedUser;
		public DbUserInfo SelectedUser {
			get => selectedUser;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedUser, value);
			}
		}

		public bool HasSelectedUser => SelectedUser != null;

		/// <summary>Форма редактирования видна, только когда выбран пользователь или создаётся новый</summary>
		public bool ShowEditForm => IsNewUser || SelectedUser != null;

		/// <summary>Подсказка в поле пароля зависит от режима формы</summary>
		public string PasswordWatermark => IsNewUser
			? "задайте пароль нового пользователя"
			: "оставьте пустым, чтобы не менять";

		public ReactiveCommand<Unit, Unit> SaveCommand { get; }
		public ReactiveCommand<Unit, Unit> BackCommand { get; }

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

		private async Task<bool> SaveUserAsync() {
			var newUser = new DbUserInfo {
				Login = EditLogin,
				Name = EditName,
				Email = EditEmail,
				Phone = EditPhone,
				Post = EditPost,
				Comment = EditComment,
				Disabled = EditDisabled,
				IsAdmin = EditIsAdmin
			};
			newUser.DirtyFields = editedDirtyFields;

			bool creating = IsNewUser;
			string password = EditNewPassword;

			try {
				await Task.Run(() => {
					if(creating)
						provider.CreateUser(newUser, password);
					else
						provider.UpdateUser(newUser, password);
				});
				interactiveMessage.ShowMessage(ImportanceLevel.Success,
					creating ? "Пользователь создан." : "Изменения сохранены.", messageTitle);
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось сохранить пользователя {0}", newUser.Login);
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, messageTitle);
				return false;
			}
			return true;
		}
		#endregion

		#region Редактируемые поля пользователя

		DbUserFields editedDirtyFields = DbUserFields.None;

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
			set {
				editedDirtyFields |= DbUserFields.Name;
				this.RaiseAndSetIfChanged(ref editName, value);
			}
		}

		private string editEmail;
		public string EditEmail {
			get => editEmail;
			set {
				editedDirtyFields |= DbUserFields.Email;
				this.RaiseAndSetIfChanged(ref editEmail, value);
			}
		}

		private string editPhone;
		public string EditPhone {
			get => editPhone;
			set {
				editedDirtyFields |= DbUserFields.Phone;
				this.RaiseAndSetIfChanged(ref editPhone, value);
			}
		}

		private string editPost;
		public string EditPost {
			get => editPost;
			set {
				editedDirtyFields |= DbUserFields.Post;
				this.RaiseAndSetIfChanged(ref editPost, value);
			}
		}

		private string editComment;
		public string EditComment {
			get => editComment;
			set {
				editedDirtyFields |= DbUserFields.Comment;
				this.RaiseAndSetIfChanged(ref editComment, value);
			}
		}

		private bool editDisabled;
		public bool EditDisabled {
			get => editDisabled;
			set {
				editedDirtyFields |= DbUserFields.Disabling;
				this.RaiseAndSetIfChanged(ref editDisabled, value);
			}
		}

		private bool editIsAdmin;
		public bool EditIsAdmin {
			get => editIsAdmin;
			set {
				editedDirtyFields |= DbUserFields.AdminFlag;
				this.RaiseAndSetIfChanged(ref editIsAdmin, value);
			}
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

			editedDirtyFields = DbUserFields.None;
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

			editedDirtyFields = DbUserFields.None;
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
					BaseAccesses.Add(new BaseAccessRowVM(row, ShowReadOnly, ShowAppPermissions));

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

		private async Task<bool> SaveAccessAsync() {
			var user = SelectedUser;
			if(user == null)
				return true;
			// Сохраняем только изменённые строки - каждая строка это отдельный запрос к серверу
			var changedRows = BaseAccesses.Where(r => r.IsDirty).ToList();
			if(changedRows.Count == 0)
				return true;
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
				return false;
			}
			return true;
		}
		#endregion

		public event Action OperationCompleted;

		private async Task Save() {
			bool isUserSavedSuccessfully = await SaveUserAsync();
			bool isBaseAccessSavedSuccessfully = true;
			if(SelectedUser != null && CanManageBaseAccess && !BaseAccessLocked) {
				isBaseAccessSavedSuccessfully = await SaveAccessAsync();
			}

			if(isUserSavedSuccessfully && isBaseAccessSavedSuccessfully) {
				PopPageCommand?.Execute(null);
				ClearEditBuffer();
				OperationCompleted?.Invoke();
			}
		}

		private async Task GoBack() {
			bool hasChanges = editedDirtyFields != DbUserFields.None
				|| !string.IsNullOrEmpty(EditNewPassword)
				|| BaseAccesses.Any(r => r.IsDirty);
			if(!hasChanges) {
				PopPageCommand?.Execute(null);
				return;
			}

			bool confirmed = await Task.Run(() =>
				interactiveQuestion.Question("Есть несохранённые изменения. Выйти без сохранения?", messageTitle));
			if(!confirmed) return;

			PopPageCommand?.Execute(null);
			ClearEditBuffer();
		}
	}
}
