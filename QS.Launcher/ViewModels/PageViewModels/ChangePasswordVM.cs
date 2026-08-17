using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class ChangePasswordVM : CarouselPageVM {
		private IDbUserManager provider;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IInteractiveMessage interactiveMessage;
		private const string MessageTitle = "Смена пароля";

		public ChangePasswordVM(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));

			var canChangeOwnPassword = this.WhenAnyValue(x => x.OwnNewPassword, x => x.OwnConfirmPassword,
				(pass, confirm) => !string.IsNullOrEmpty(pass) && pass == confirm);
			ChangeOwnPasswordCommand = ReactiveCommand.CreateFromTask(
				() => RunBusyAsync(MessageTitle, ChangeOwnPasswordAsync), canChangeOwnPassword);
			BackCommand = ReactiveCommand.Create(() => PopPageCommand?.Execute(null));
		}

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
		public ReactiveCommand<Unit,Unit> BackCommand { get; }
		public void SetProvider(IDbUserManager userManager) {
			provider = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		private async Task ChangeOwnPasswordAsync() {
			try {
				string newPassword = OwnNewPassword;
				bool ok = await Task.Run(() => provider.ChangeOwnPassword(newPassword));
				// со страницы уходим только когда пароль действительно сменился:
				// иначе пользователь теряет форму вместе с сообщением об ошибке
				if(!ok) {
					interactiveMessage.ShowMessage(ImportanceLevel.Error, "Не удалось изменить пароль.", MessageTitle);
					return;
				}

				OwnNewPassword = null;
				OwnConfirmPassword = null;
				interactiveMessage.ShowMessage(ImportanceLevel.Success, "Пароль изменён.", MessageTitle);
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось сменить собственный пароль");
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Смена пароля");
				return;
			}

			PopPageCommand?.Execute(null);
		}
	}
}
