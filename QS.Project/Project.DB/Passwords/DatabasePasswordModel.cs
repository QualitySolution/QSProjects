using System;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using NLog;
using QS.DomainModel.UoW;

namespace QS.Project.DB.Passwords
{
    public class DatabasePasswordModel : IDatabasePasswordModel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public void ChangePassword(DbConnection connection, string newPassword)
        {
            if(connection.State != ConnectionState.Open) {
                throw new ArgumentException("Connection is not open");
            }
            if(!(connection is MySqlConnection)) {
                throw new NotSupportedException("Only MySqlConnection is suppoted");
            }
            string login;
            var builder = new DbConnectionStringBuilder { ConnectionString = connection.ConnectionString };
            if(builder.TryGetValue("user id", out var databaseAsObject)) {
                login = databaseAsObject.ToString();
            }
            else {
                throw new ArgumentException($"Не удалось достать user id из {nameof(DbConnection)}", nameof(connection));
            }
            
            logger.Debug($"Смена пароля для пользователя '{login}'...");
            
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SET PASSWORD = PASSWORD('{newPassword}')";
            cmd.ExecuteNonQuery();

            logger.Debug("OK");
        }

        public void ChangePassword(IUnitOfWork uow, string newPassword)
        {
            ChangePassword(uow.Session.Connection, newPassword);
        }

        public bool IsCurrentUserPassword(DbConnection connection, string password)
        {
            if(connection.State != ConnectionState.Open) {
                throw new ArgumentException("Connection is not open");
            }
            if(!(connection is MySqlConnection)) {
                throw new NotSupportedException("Only MySqlConnection is supported");
            }
            string login;
            var builder = new DbConnectionStringBuilder { ConnectionString = connection.ConnectionString };
            if(builder.TryGetValue("user id", out var databaseAsObject)) {
                login = databaseAsObject.ToString();
            }
            else {
                throw new ArgumentException($"Не удалось достать user id из {nameof(DbConnection)}", nameof(connection));
            }

            logger.Debug($"Проверка текущего пароля пользователя '{login}'...");

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT" +
                    "(SELECT mysql.user.Password " +
                        "FROM mysql.user " +
                        "WHERE mysql.user.User = SUBSTRING_INDEX(CURRENT_USER(), '@', 1)" +
                        "AND mysql.user.Host = SUBSTRING_INDEX(CURRENT_USER(), '@', -1) LIMIT 1) " +
                    "= " +
                    $"PASSWORD('{password}');";
            var res = (long)cmd.ExecuteScalar();
            
            logger.Debug("OK");

            return res != 0;
        }

        public bool IsCurrentUserPassword(IUnitOfWork uow, string password)
        {
            return IsCurrentUserPassword(uow.Session.Connection, password);
        }
    }
}
