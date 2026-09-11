namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// Низкоуровневая модель наполнения БД для конкретного движка
	/// </summary>
	public interface IDbCreatorModel
	{
		bool RunCreation(string dbName, string dbTitle);
	}
}
