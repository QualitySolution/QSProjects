using System.Threading.Tasks;

namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// Универсальный контракт создания и наполнения базы данных.
	/// </summary>
	public interface IDBCreator
	{
		Task<bool> RunCreationAsync(string dbName, string dbTitle);
	}
}
