using System.Linq;
using Gtk;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels;
using Key = Gdk.Key;
using QS.Views.Dialog;

namespace QS.ChangePassword.Views
{
    public partial class ChangePasswordView : DialogViewBase<ChangePasswordViewModel>
    {
		private readonly Image _showCurrentPasswordIcon;
		private readonly Image _showNewPasswordIcon;
		private readonly Image _hideCurrentPasswordIcon;
		private readonly Image _hideNewPasswordIcon;

		public ChangePasswordView(ChangePasswordViewModel viewModel) : base(viewModel)
        {
            Build();

            var eyePix = Gdk.Pixbuf.LoadFromResource("QS.Icons.Eye.png");
            var crossedEyePix = Gdk.Pixbuf.LoadFromResource("QS.Icons.CrossedEye.png");

			_showCurrentPasswordIcon = new Image { Pixbuf = eyePix };
            _showNewPasswordIcon = new Image { Pixbuf = eyePix };
			_hideCurrentPasswordIcon = new Image { Pixbuf = crossedEyePix };
			_hideNewPasswordIcon = new Image { Pixbuf = crossedEyePix };

            Configure();
        }

		private void Configure()
        {
			btnSave.CanFocus = false;
			btnSave.Clicked += (sender, e) => ViewModel.Save();
			btnSave.Binding
				.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

            btnCancel.CanFocus = false;
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);

            entryCurrentPassword.Changed += (sender, e) => ViewModel.OldPassword = entryCurrentPassword.Text?.ToSecureString();
            entryNewPassword.Changed += (sender, e) => ViewModel.NewPassword = entryNewPassword.Text?.ToSecureString();
			entryConfirmNewPassword.Changed +=
                (sender, args) => ViewModel.NewPasswordConfirm = entryConfirmNewPassword.Text?.ToSecureString();

            ytoggleShowCurrentPassword.CanFocus = false;
			ytoggleShowCurrentPassword.Toggled += (sender, args) => {
                var active = ytoggleShowCurrentPassword.Active;
                entryCurrentPassword.Visibility = active;
				ytoggleShowCurrentPassword.Image = active ? _hideCurrentPasswordIcon : _showCurrentPasswordIcon;
            };

            ytoggleShowPassword.CanFocus = false;
            ytoggleShowPassword.Toggled += (sender, args) => {
                var active = ytoggleShowPassword.Active;
                entryNewPassword.Visibility = active;
                ytoggleShowPassword.Image = active ? _hideNewPasswordIcon : _showNewPasswordIcon;
            };

            ViewModel.PropertyChanged += (sender, args) => {
                switch(args.PropertyName) {
                    case nameof(ViewModel.ValidationStatus):
                        UpdateValidationStatus();
						break;
                }
            };

            KeyReleaseEvent += (o, args) => {
                if(args.Event.Key == Key.Return && ViewModel.CanSave) {
                    btnSave.Click();
                }
            };

            InitializeValidationLabels();
            UpdateValidationStatus();
        }

        private void InitializeValidationLabels()
        {
            for(int i = 0; i < ViewModel.MaxMessages; i++) {
                vboxValidationResult.Add(new Label { UseMarkup = true });
            }
        }

        private void UpdateValidationStatus()
        {
            var colorName = ViewModel.CanSave ? "green" : "red";
            var messages = ViewModel.ValidationStatus.Split('\n');
            var labels = vboxValidationResult.Children.OfType<Label>().ToArray();
            for(int i = 0; i < labels.Length; i++) {
                labels[i].Markup = messages.Length > i ? $"<span foreground=\"{colorName}\">{messages[i]}</span>" : "";
            }
        }
    }
}
