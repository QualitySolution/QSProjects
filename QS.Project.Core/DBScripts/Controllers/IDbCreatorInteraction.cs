namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// всплывающие окна у пользователя при уточнениях
	/// </summary>
	public interface IDbCreatorInteraction
	{
		bool AskDropExistingDatabase(string dbName);

		void ReportError(string text, string lastExecutedStatement);
	}
}
