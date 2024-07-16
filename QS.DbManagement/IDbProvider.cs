namespace QS.DbManagement;

public interface IDbProvider : IDisposable
{
	public string ConnectionString { get; set; }

	public bool ChangePassword(string oldPassword, string newPassword);
		   
	public bool CreateDatabase(string databaseName);
		   
	public bool DropDatabase(string databaseName);
		   
	public bool AddUser(string username, string password);

	public bool IsConnected { get; }
}
