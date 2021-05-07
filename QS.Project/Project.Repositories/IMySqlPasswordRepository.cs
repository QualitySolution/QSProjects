using System.Data.Common;
using MySql.Data.MySqlClient;

namespace QS.Project.Repositories
{
    public interface IMySqlPasswordRepository
    {
        /// <summary>
        /// Меняет пароль для пользователя из переданного <see cref="connection"/>
        /// </summary>
        void ChangeConnectionUserPassword(MySqlConnection connection, string newPassword);
        
        /// <summary>
        /// Проверяет, является ли переданный <see cref="password"/> текущим паролем пользователя из переданного <see cref="connection"/>
        /// </summary>
        bool IsConnectionUserPassword(MySqlConnection connection, string password);
    }
}
