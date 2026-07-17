using Gamma.Utilities;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Linq;

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

		public ToDoWithExistingDatabase AskDropExistingDatabase(string dbName)
		{
			ToDoWithExistingDatabase[] options = new ToDoWithExistingDatabase[] { ToDoWithExistingDatabase.Rewrite, ToDoWithExistingDatabase.Recreate, ToDoWithExistingDatabase.Nothing };
			var buttons = options.Select(o => o.GetEnumTitle()).ToArray();
			string response = question.Question(buttons,
				$"База с именем `{dbName}` уже существует на сервере.\n" +
				"Перезаписать - заменить содержимое базы, сохранив пользователей.\n" +
				"Пересоздать - полностью удалить базу и создать заново.",
				"Создание базы данных");
			int idx = Array.IndexOf(buttons, response);
			return idx >= 0 ? options[idx] : ToDoWithExistingDatabase.Nothing; // null при закрытии крестиком → Nothing
		}

		public void ReportError(string text, string lastExecutedStatement) {
			message.ShowMessage(ImportanceLevel.Error, text, "Ошибка создания базы");
		}
	}
}
