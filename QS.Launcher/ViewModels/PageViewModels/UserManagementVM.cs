using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Project.Versioning;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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
			LauncherNavigation navigation,
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			IErrorHandlingService errorHandling) : base(navigation) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.errorHandling = errorHandling ?? throw new ArgumentNullException(nameof(errorHandling));

			Card = new UserCardVM();
			BaseAccesses = new ObservableCollection<BaseAccessRowVM>();

			var canSaveUser = this.WhenAnyValue(x => x.Card.Login, x => x.Card.NewPassword, x => x.Card.IsNew,
				(login, pass, isNew) => !string.IsNullOrWhiteSpace(login) && (!isNew || !string.IsNullOrEmpty(pass)));
			SaveCommand = ReactiveCommand.CreateFromTask(Save, canSaveUser);

			BackCommand = ReactiveCommand.CreateFromTask(GoBack);

			LoadSelectedUserCommand = ReactiveCommand.CreateFromTask<DbUserInfo>(LoadSelectedUser);

			TrackBusy(SaveCommand, LoadSelectedUserCommand);

			this.WhenAnyValue(x => x.SelectedUser).InvokeCommand(LoadSelectedUserCommand);

			this.WhenAnyValue(x => x.SelectedUser, x => x.Card.IsNew)
				.Subscribe(_ => this.RaisePropertyChanged(nameof(ShowEditForm)));
		}

		public void SetContext(IDbUserManager userManager, DbUserInfo user, bool isCreating) {
			provider = userManager ?? throw new ArgumentNullException(nameof(userManager));

			Card.ApplySupportedFields(provider.SupportedUserFields);
			ShowReadOnly = provider.SupportedUserFields.HasFlag(DbUserFields.BaseReadOnly);

			if(isCreating)
				Card.IsNew = true;
			else
				SelectedUser = user;
		}

		#region Управление пользователями

		private UserCardVM card;
		public UserCardVM Card {
			get => card;
			private set => this.RaiseAndSetIfChanged(ref card, value);
		}

		public bool CanManageUsers => provider?.CanManageUsers == true;

		/// <summary>Может ли текущий пользователь сохранять доступы к базам</summary>
		public bool CanManageBaseAccess => provider?.CanManageBaseAccess == true;

		public bool ShowReadOnly { get; private set; }

		private DbUserInfo selectedUser;
		public DbUserInfo SelectedUser {
			get => selectedUser;
			set => this.RaiseAndSetIfChanged(ref selectedUser, value);
		}

		public bool HasSelectedUser => SelectedUser != null;

		public bool ShowEditForm => Card.IsNew || SelectedUser != null;

		public ReactiveCommand<Unit, Unit> SaveCommand { get; }
		public ReactiveCommand<Unit, Unit> BackCommand { get; }

		public ReactiveCommand<DbUserInfo, Unit> LoadSelectedUserCommand { get; }

		private async Task LoadSelectedUser(DbUserInfo user) {
			this.RaisePropertyChanged(nameof(HasSelectedUser));
			BaseAccesses.Clear();
			this.RaisePropertyChanged(nameof(BaseAccessLocked));
			Card.IsNew = false;
			if(user == null)
				return;

			Card.Load(user);
			await LoadBaseAccess(user.Login);
			// снимок снимаем после LoadBaseAccess
			Card.AcceptChanges();
		}

		private async Task<bool> SaveUserAsync() {
			var newUser = Card.ToUser();

			bool creating = Card.IsNew;
			string password = Card.NewPassword;

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

		#region Доступ к базам

		public ObservableCollection<BaseAccessRowVM> BaseAccesses { get; }

		public bool BaseAccessLocked => BaseAccesses.Count > 0 && BaseAccesses.All(r => !r.CanEdit);

		private async Task LoadBaseAccess(string login) {
			try {
				// чтение блокирующее и срабатывает на каждый выбор в списке - уводим в фон
				List<DbUserBaseAccess> rows = await Task.Run(() => provider.GetUserBaseAccess(login));

				BaseAccesses.Clear();
				foreach(var row in rows)
					BaseAccesses.Add(new BaseAccessRowVM(row, ShowReadOnly));

				var byBase = rows.OrderBy(r => r.BaseName, StringComparer.Ordinal).ToList();

				if(string.IsNullOrEmpty(Card.Name))
					Card.Name = byBase.FirstOrDefault(n => !string.IsNullOrEmpty(n.Name))?.Name;
				if(string.IsNullOrEmpty(Card.Email))
					Card.Email = byBase.FirstOrDefault(e => !string.IsNullOrEmpty(e.Email))?.Email;
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
			string name = Card.Name;
			string email = Card.Email;
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
		private bool NeedSaveUser => Card.IsNew || Card.IsDirty || !string.IsNullOrEmpty(Card.NewPassword);

		private async Task Save() {
			bool isBaseAccessSavedSuccessfully = true;
			if(SelectedUser != null && CanManageBaseAccess && !BaseAccessLocked) {
				isBaseAccessSavedSuccessfully = await SaveAccessAsync();
			}

			bool isUserSavedSuccessfully = !NeedSaveUser
				|| await SaveUserAsync();

			if(isUserSavedSuccessfully && isBaseAccessSavedSuccessfully) {
				Navigation.Pop();
				OperationCompleted?.Invoke();
			}
		}

		private async Task GoBack() {
			bool hasChanges = Card.IsDirty
				|| !string.IsNullOrEmpty(Card.NewPassword)
				|| BaseAccesses.Any(r => r.IsDirty);
			if(!hasChanges) {
				Navigation.Pop();
				return;
			}

			bool confirmed = await interactiveQuestion.AskInBackground(
				"Есть несохранённые изменения. Выйти без сохранения?", messageTitle);
			if(!confirmed) return;

			Navigation.Pop();
		}
	}
}
