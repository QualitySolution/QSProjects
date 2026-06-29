namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// Низкоуровневая модель наполнения БД для конкретного движка
	/// </summary>
	public interface IDbCreatorModel
	{
		// Метод блокирует вызывающий поток на время работы с базой
		// Вынесение в фоновый поток — ответственность вызывающего кода
		bool RunCreation(string dbName, string dbTitle);
	}
}
