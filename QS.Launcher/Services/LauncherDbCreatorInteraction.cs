using System;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.Launcher.Services {
	public class LauncherDbCreatorInteraction : IDbCreatorInteraction {
		private readonly IInteractiveQuestion question;
		private readonly IInteractiveMessage message;

		public LauncherDbCreatorInteraction(
			IInteractiveQuestion question,
			IInteractiveMessage message)
		{
			this.question = question ?? throw new ArgumentNullException(nameof(question));
			this.message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public bool AskDropExistingDatabase(string dbName) {
			return question.Question(
				$"База с именем `{dbName}` уже существует на сервере. Удалить существующую базу перед созданием новой?",
				"Создание базы данных");
		}

		public void ReportError(string text, string lastExecutedStatement) {
			message.ShowMessage(ImportanceLevel.Error, text, "Ошибка создания базы");
		}
	}
}
