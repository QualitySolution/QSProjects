using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using QS.Navigation;
using QS.Project.DB.Passwords;
using QS.Utilities.Text;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.ViewModels
{
    public class ChangePasswordViewModel : DialogViewModelBase
    {
        public ChangePasswordViewModel(
            IChangePasswordModel changePasswordModel,
            IPasswordValidator passwordValidator, 
            INavigationManager navigation)
            : base(navigation)
        {
            this.changePasswordModel = changePasswordModel ?? throw new ArgumentNullException(nameof(changePasswordModel));
            this.passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
            OnPasswordUpdated();
        }

        private readonly IChangePasswordModel changePasswordModel;
        private readonly IPasswordValidator passwordValidator;
        
        public int? MaxMessages => 3;

        private bool canSave;
        public bool CanSave {
            get => canSave;
            set => SetField(ref canSave, value);
        }

        private string validationStatus;
        public string ValidationStatus {
            get => validationStatus;
            set => SetField(ref validationStatus, value);
        }

        private SecureString oldPassword = new SecureString();
        public SecureString OldPassword {
            get => oldPassword;
            set {
                if(SetField(ref oldPassword, value)) {
                    OnPasswordUpdated();
                }
            }
        }

        private SecureString newPassword = new SecureString();
        public SecureString NewPassword {
            get => newPassword;
            set {
                if(SetField(ref newPassword, value)) {
                    OnPasswordUpdated();
                }
            }
        }

        private SecureString newPasswordConfirm = new SecureString();
        public SecureString NewPasswordConfirm {
            get => newPasswordConfirm;
            set {
                if(SetField(ref newPasswordConfirm, value)) {
                    OnPasswordUpdated();
                }
            }
        }

        public bool Save()
        {
            if(!CanSave) {
                return false;
            }
            if(!changePasswordModel.IsCurrentUserPassword(OldPassword)) {
                SetValidationResult(false, "Неверно введён текущий пароль");
                return false;
            }
            if(OldPassword.ToPlainString().Equals(NewPassword.ToPlainString())) {
                SetValidationResult(false, "Новый пароль не должен совпадать со старым");
                return false;
            }
            changePasswordModel.ChangePassword(newPassword);
            return true;
        }

        private void OnPasswordUpdated()
        {
            var errorMessages = new List<string>();
            if(!passwordValidator.Validate(NewPassword?.ToPlainString(), out var messages)) {
                errorMessages.AddRange(messages);
            }
            if(newPassword?.ToPlainString() != newPasswordConfirm?.ToPlainString()) {
                errorMessages.Add("Пароли не совпадают");
            }

            if(errorMessages.Any()) {
                SetValidationResult(false, String.Join("\n", MaxMessages.HasValue ? errorMessages.Take(MaxMessages.Value) : errorMessages));
            }
            else {
                SetValidationResult(true, "ok");
            }
        }

        private void SetValidationResult(bool isValid, string message)
        {
            CanSave = isValid;
            ValidationStatus = message;
        }
    }
}
