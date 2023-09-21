using MySqlConnector;

namespace QS.Project.Repositories
{
    public class MySqlPasswordRepository : IMySqlPasswordRepository
    {
        public void ChangeConnectionUserPassword(MySqlConnection connection, string newPassword)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SET PASSWORD = PASSWORD('{newPassword}')";
            cmd.ExecuteNonQuery();
        }
        
        public bool IsConnectionUserPassword(MySqlConnection connection, string password)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(connection.ConnectionString) { Password = password };
            using(var testConnection = new MySqlConnection(connectionStringBuilder.ConnectionString)) {
                try {
                    //Если можно открыть конненкш с переданным паролем, значит передан текущий пароль
                    testConnection.Open();
                    testConnection.Close();
                    return true;
                }
                catch(MySqlException ex) {
                    if(ex.Number == 1045 || ex.InnerException is MySqlException innerEx && innerEx.Number == 1045) {
                        return false;
                    }
                    throw;
                }
            }
        }
    }
}
