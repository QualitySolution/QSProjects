using MySql.Data.MySqlClient;

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
            return res != 0;
        }
    }
}
