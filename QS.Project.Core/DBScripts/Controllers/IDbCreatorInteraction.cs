using System.Threading.Tasks;

namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// всплывающие окна у пользователя при уточнениях
	/// </summary>
	public interface IDbCreatorInteraction
	{
		Task<bool> AskDropExistingDatabaseAsync(string dbName);

		Task ReportErrorAsync(string text, string lastExecutedStatement);
	}
}
