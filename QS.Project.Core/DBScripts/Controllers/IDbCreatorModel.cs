using System.Threading.Tasks;

namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// Низкоуровневая модель создания БД: знает, как физически создать и наполнить базу
	/// </summary>
	public interface IDbCreatorModel
	{
		Task<bool> RunCreationAsync(string dbName, string dbTitle);
	}
}
