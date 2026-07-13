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

		public ChangePasswordVM(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));

			var canChangeOwnPassword = this.WhenAnyValue(x => x.OwnNewPassword, x => x.OwnConfirmPassword,
				(pass, confirm) => !string.IsNullOrEmpty(pass) && pass == confirm);
			ChangeOwnPasswordCommand = ReactiveCommand.CreateFromTask(ChangeOwnPasswordAsync, canChangeOwnPassword);
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
				return;
			}

			PopPageCommand?.Execute(null);
		}
	}
}
