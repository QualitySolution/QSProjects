using System;
using System.Linq;
using Gtk;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels;
using Key = Gdk.Key;

namespace QS.ChangePassword.Views
{
    public partial class ChangePasswordView : Gtk.Dialog
    {
        public ChangePasswordView(ChangePasswordViewModel viewModel)
        {
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Build();
            Title = "Смена пароля";

            var eyePix = Gdk.Pixbuf.LoadFromResource("QS.Icons.Eye.png");
            var crossedEyePix = Gdk.Pixbuf.LoadFromResource("QS.Icons.CrossedEye.png");

            eyeIcon = new Image { Pixbuf = eyePix };
            eyeIcon2 = new Image { Pixbuf = eyePix };
            crossedEyeIcon = new Image { Pixbuf = crossedEyePix };
            crossedEyeIcon2 = new Image { Pixbuf = crossedEyePix };

            Configure();
        }

        private readonly ChangePasswordViewModel viewModel;

        private readonly Image eyeIcon;
        private readonly Image crossedEyeIcon;

        private readonly Image eyeIcon2;
        private readonly Image crossedEyeIcon2;

        private void Configure()
        {
            buttonOK.CanFocus = false;
            buttonOK.Clicked += (sender, args) => {
                if(viewModel.Save()) {
                    var button = new Button();
                    AddActionWidget(button, ResponseType.Ok);
                    button.Click();
                }
            };
            buttonOK.Sensitive = viewModel.CanSave;

            buttonCancel.CanFocus = false;
            buttonCancel.Clicked += (sender, args) => viewModel.Close(false, CloseSource.Cancel);

            yentryOldPassword.Changed += (sender, args) => viewModel.OldPassword = yentryOldPassword.Text?.ToSecureString();
            yentryNewPassword.Changed += (sender, args) => viewModel.NewPassword = yentryNewPassword.Text?.ToSecureString();
            yentryNewPasswordConfirm.Changed +=
                (sender, args) => viewModel.NewPasswordConfirm = yentryNewPasswordConfirm.Text?.ToSecureString();

            ytoggleShowOldPassword.CanFocus = false;
            ytoggleShowOldPassword.Toggled += (sender, args) => {
                var active = ytoggleShowOldPassword.Active;
                yentryOldPassword.Visibility = active;
                ytoggleShowOldPassword.Image = active ? crossedEyeIcon : eyeIcon;
            };

            ytoggleShowPassword.CanFocus = false;
            ytoggleShowPassword.Toggled += (sender, args) => {
                var active = ytoggleShowPassword.Active;
                yentryNewPassword.Visibility = active;
                ytoggleShowPassword.Image = active ? crossedEyeIcon2 : eyeIcon2;
            };

            viewModel.PropertyChanged += (sender, args) => {
                switch(args.PropertyName) {
                    case nameof(viewModel.ValidationStatus):
                        UpdateValidationStatus();
                        break;
                    case nameof(viewModel.CanSave):
                        buttonOK.Sensitive = viewModel.CanSave;
                        break;
                }
            };
            KeyReleaseEvent += (o, args) => {
                if(args.Event.Key == Key.Return && viewModel.CanSave) {
                    buttonOK.Click();
                }
            };

            InitializeValidationLabels();
            UpdateValidationStatus();
        }

        private void InitializeValidationLabels()
        {
            for(int i = 0; i < viewModel.MaxMessages; i++) {
                vboxValidationResult.Add(new Label { UseMarkup = true });
            }
        }

        private void UpdateValidationStatus()
        {
            var colorName = viewModel.CanSave ? "green" : "red";
            var messages = viewModel.ValidationStatus.Split('\n');
            var labels = vboxValidationResult.Children.OfType<Label>().ToArray();
            for(int i = 0; i < labels.Length; i++) {
                labels[i].Markup = messages.Length > i ? $"<span foreground=\"{colorName}\">{messages[i]}</span>" : "";
            }
        }
    }
}
