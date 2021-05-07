using System;
using System.Data;
using System.Security;
using MySql.Data.MySqlClient;
using NLog;
using QS.Project.Repositories;
using QS.Utilities.Text;

namespace QS.Project.DB.Passwords
{
    public class MySqlChangePasswordModel : IChangePasswordModel
    {
        public MySqlChangePasswordModel(MySqlConnection connection, IMySqlPasswordRepository mySqlPasswordRepository)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.mySqlPasswordRepository = mySqlPasswordRepository ?? throw new ArgumentNullException(nameof(mySqlPasswordRepository));
        }
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly MySqlConnection connection;
        private readonly IMySqlPasswordRepository mySqlPasswordRepository;
        public virtual bool PasswordWasChanged { get; private set; }
        public virtual SecureString NewPassword { get; private set; }

        public virtual void ChangePassword(SecureString newPassword)
        {
            if(connection.State != ConnectionState.Open) {
                throw new ArgumentException("Connection is not open");
            }

            var builder = new MySqlConnectionStringBuilder { ConnectionString = connection.ConnectionString };
            var userId = builder.UserID;
            logger.Debug($"Смена пароля для пользователя '{userId}'...");
            
            mySqlPasswordRepository.ChangeConnectionUserPassword(connection, newPassword.ToPlainString());

            NewPassword = newPassword;
            PasswordWasChanged = true;
            
            logger.Debug("OK");
        }

        public virtual bool IsCurrentUserPassword(SecureString password)
        {
            if(connection.State != ConnectionState.Open) {
                throw new ArgumentException("Connection is not open");
            }
            var builder = new MySqlConnectionStringBuilder { ConnectionString = connection.ConnectionString };
            var userId = builder.UserID;

            logger.Debug($"Проверка текущего пароля пользователя '{userId}'...");

            var res = mySqlPasswordRepository.IsConnectionUserPassword(connection, password.ToPlainString());

            logger.Debug("OK");

            return res;
        }
    }
}
