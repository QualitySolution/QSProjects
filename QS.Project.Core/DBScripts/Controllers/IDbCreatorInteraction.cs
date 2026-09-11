using System.ComponentModel.DataAnnotations;

namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// всплывающие окна у пользователя при уточнениях
	/// </summary>
	public interface IDbCreatorInteraction
	{
		ToDoWithExistingDatabase AskDropExistingDatabase(string dbName);

		void ReportError(string text, string message);
	}

	public enum ToDoWithExistingDatabase
	{
		[Display(Name = "Ничего не делать")]
		Nothing,
		[Display(Name = "Перезаписать")]
		Rewrite,
		[Display(Name = "Пересоздать")]
		Recreate
	}
}
