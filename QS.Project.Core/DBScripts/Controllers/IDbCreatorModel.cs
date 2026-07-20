using System;

namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// Низкоуровневая модель наполнения БД для конкретного движка
	/// </summary>
	public interface IDbCreatorModel
	{
		bool RunCreation(string dbName, string dbTitle);

		/// <summary>Версия базы, которую даст наполнение. null - наполнителю версия неизвестна (например дамп)</summary>
		Version NewBaseVersion { get; }
	}
}
