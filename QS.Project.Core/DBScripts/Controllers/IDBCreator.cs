namespace QS.DBScripts.Controllers
{
	public interface IDBCreator
	{
		void RunCreation(string server, string dbname);
	}
}