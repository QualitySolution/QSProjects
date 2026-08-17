using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.ErrorReporting;
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
		private readonly IErrorHandlingService errorHandling;

		private readonly string messageTitle = "Управление пользователями";
		private const string AccessTitle = "Доступ к базам";

		private IDbUserManager provider;

		public UserManagementVM(
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			IErrorHandlingService errorHandling) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.errorHandling = errorHandling ?? throw new ArgumentNullException(nameof(errorHandling));

			BaseAccesses = new ObservableCollection<BaseAccessRowVM>();

			var canSaveUser = this.WhenAnyValue(x => x.EditLogin, x => x.EditNewPassword, x => x.IsNewUser,
				(login, pass, isNew) => !string.IsNullOrWhiteSpace(login) && (!isNew || !string.IsNullOrEmpty(pass)));
			SaveCommand = ReactiveCommand.CreateFromTask(
				() => RunBusyAsync("Сохранение пользователя", Save), canSaveUser);

			BackCommand = ReactiveCommand.CreateFromTask(GoBack);

			// загрузка карточки ходит в базу, поэтому она команда, а не обработчик подписки:
			// команда исполняется асинхронно и сама отдаёт ошибку общему обработчику
			LoadSelectedUserCommand = ReactiveCommand.CreateFromTask<DbUserInfo>(
				user => RunBusyAsync("Загрузка карточки пользователя", () => LoadSelectedUser(user)));
			this.WhenAnyValue(x => x.SelectedUser).InvokeCommand(LoadSelectedUserCommand);

			this.WhenAnyValue(x => x.SelectedUser, x => x.IsNewUser)
				.Subscribe(_ => {
					this.RaisePropertyChanged(nameof(ShowEditForm));
					this.RaisePropertyChanged(nameof(PasswordWatermark));
				});
		}

		public void SetContext(IDbUserManager userManager, DbUserInfo user, bool isCreating) {
			provider = userManager ?? throw new ArgumentNullException(nameof(userManager));

			if(isCreating)
				IsNewUser = true;
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

		public ReactiveCommand<Unit, Unit> SaveCommand { get; }
		public ReactiveCommand<Unit, Unit> BackCommand { get; }

		public ReactiveCommand<DbUserInfo, Unit> LoadSelectedUserCommand { get; }

		private async Task LoadSelectedUser(DbUserInfo user) {
			this.RaisePropertyChanged(nameof(HasSelectedUser));
			BaseAccesses.Clear();
			this.RaisePropertyChanged(nameof(BaseAccessLocked));
			IsNewUser = false;
			if(user == null)
				return;

			LoadEditBuffer(user);
			await LoadBaseAccess(user.Login);
			// снимок снимаем после LoadBaseAccess: он дозаполняет имя и почту из таблицы users,
			// и эта подстановка правкой пользователя не является
			AcceptUserChanges();
		}

		private async Task<bool> SaveUserAsync() {
			var newUser = EditedUser();

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
				errorHandling.Handle(ex, messageTitle);
				return false;
			}
			return true;
		}
		#endregion

		#region Редактируемые поля пользователя

		/// <summary>Собранный из полей формы пользователь - и на сохранение, и для сравнения со снимком</summary>
		private DbUserInfo EditedUser() => new DbUserInfo {
			Login = EditLogin,
			Name = EditName,
			Email = EditEmail,
			Phone = EditPhone,
			Post = EditPost,
			Comment = EditComment,
			Disabled = EditDisabled,
			IsAdmin = EditIsAdmin
		};

		private const string SignatureSeparator = "\u0001";

		private static string Signature(DbUserInfo user) => string.Join(SignatureSeparator,
			user.Login, user.Name, user.Email, user.Phone, user.Post, user.Comment,
			user.Disabled ? "1" : "0", user.IsAdmin ? "1" : "0");

		private string loadedSignature = Signature(new DbUserInfo());

		private bool IsUserDirty => Signature(EditedUser()) != loadedSignature;

		private void AcceptUserChanges() => loadedSignature = Signature(EditedUser());

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

			AcceptUserChanges();
		}

		#endregion

		#region Доступ к базам

		public ObservableCollection<BaseAccessRowVM> BaseAccesses { get; }

		// Доступ пользователя следует из глобальных прав на сервер и точечно не настраивается
		public bool BaseAccessLocked => BaseAccesses.Count > 0 && BaseAccesses.All(r => !r.CanEdit);

		private async Task LoadBaseAccess(string login) {
			try {
				// чтение блокирующее и срабатывает на каждый выбор в списке - уводим в фон
				var rows = await Task.Run(() => provider.GetUserBaseAccess(login));
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
				errorHandling.Handle(ex, AccessTitle);
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
			string name = EditName;
			string email = EditEmail;
			try {
				await Task.Run(() => {
					foreach(var row in changedRows)
						provider.SetUserBaseAccess(user.Login, row.ToAccess(name, email));
				});
				foreach(var row in changedRows)
					row.AcceptChanges();
				interactiveMessage.ShowMessage(ImportanceLevel.Success, "Доступы сохранены", AccessTitle);
			}
			catch(Exception ex) {
				errorHandling.Handle(ex, AccessTitle);
				return false;
			}
			return true;
		}
		#endregion

		public event Action OperationCompleted;

		/// <summary>Писать пользователя незачем, если карточку не трогали и пароль не задан</summary>
		private bool NeedSaveUser => IsNewUser || IsUserDirty || !string.IsNullOrEmpty(EditNewPassword);

		private async Task Save() {
			bool isBaseAccessSavedSuccessfully = true;
			if(SelectedUser != null && CanManageBaseAccess && !BaseAccessLocked) {
				isBaseAccessSavedSuccessfully = await SaveAccessAsync();
			}

			bool isUserSavedSuccessfully = !NeedSaveUser || await SaveUserAsync();

			if(isUserSavedSuccessfully && isBaseAccessSavedSuccessfully) {
				PopPageCommand?.Execute(null);
				OperationCompleted?.Invoke();
			}
		}

		private async Task GoBack() {
			bool hasChanges = IsUserDirty
				|| !string.IsNullOrEmpty(EditNewPassword)
				|| BaseAccesses.Any(r => r.IsDirty);
			if(!hasChanges) {
				PopPageCommand?.Execute(null);
				return;
			}

			bool confirmed = await interactiveQuestion.AskInBackground(
				"Есть несохранённые изменения. Выйти без сохранения?", messageTitle);
			if(!confirmed) return;

			PopPageCommand?.Execute(null);
		}
	}
}
