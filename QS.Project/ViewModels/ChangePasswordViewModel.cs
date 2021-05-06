using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.DB.Passwords;
using QS.Utilities.Text;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.ViewModels
{
    public class ChangePasswordViewModel : DialogViewModelBase
    {
        public ChangePasswordViewModel(IDatabasePasswordModel databasePasswordModel, DbConnection dbConnection,
            IPasswordValidator passwordValidator, INavigationManager navigation)
            : base(navigation)
        {
            this.databasePasswordModel = databasePasswordModel ?? throw new ArgumentNullException(nameof(databasePasswordModel));
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            this.passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
            OnPasswordUpdated();
        }

        private readonly IDatabasePasswordModel databasePasswordModel;
        private readonly DbConnection dbConnection;
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

        private SecureString oldPassword;
        public SecureString OldPassword {
            get => oldPassword;
            set {
                if(SetField(ref oldPassword, value)) {
                    OnPasswordUpdated();
                }
            }
        }

        private SecureString newPassword;
        public SecureString NewPassword {
            get => newPassword;
            set {
                if(SetField(ref newPassword, value)) {
                    OnPasswordUpdated();
                }
            }
        }

        private SecureString newPasswordConfirm;
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
            if(!databasePasswordModel.IsCurrentUserPassword(dbConnection, OldPassword?.ToPlainString())) {
                SetValidationResult(false, "Неверно введён текущий пароль");
                return false;
            }
            if(databasePasswordModel.IsCurrentUserPassword(dbConnection, NewPassword?.ToPlainString())) {
                SetValidationResult(false, "Новый пароль не должен совпадать со старым");
                return false;
            }
            SaveNewPassword();
            return true;
        }

        private void SaveNewPassword()
        {
            databasePasswordModel.ChangePassword(dbConnection, newPassword?.ToPlainString());
            var dbConnectionStringBuilder = new DbConnectionStringBuilder {
                ConnectionString = Connection.ConnectionString,
                ["Password"] = newPassword?.ToPlainString()
            };

            //Заменяем пароль с текущей строке одключения, для того чтобы при переподключении не было ошибок 
            //и чтобы при смене пароля еще раз был текущий пароль.
            Connection.ChangeDbConnectionString(dbConnectionStringBuilder.ConnectionString);
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
                SetValidationResult(true, "🗸");
            }
        }

        private void SetValidationResult(bool isValid, string message)
        {
            CanSave = isValid;
            ValidationStatus = message;
        }
    }
}
