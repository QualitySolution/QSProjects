using System;
using System.Threading.Tasks;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.Launcher.Services {
	/// <summary>
	/// проксирует вопросы и ошибки 
	/// все вызовы диалогов уходят в UI-поток через IUiThreadInvoker, 
	/// потому что creator дёргает их с фонового потока
	/// </summary>
	public class LauncherDbCreatorInteraction : IDbCreatorInteraction {
		private readonly IInteractiveQuestion question;
		private readonly IInteractiveMessage message;
		private readonly IUiThreadInvoker uiThread;

		public LauncherDbCreatorInteraction(
			IInteractiveQuestion question,
			IInteractiveMessage message,
			IUiThreadInvoker uiThread)
		{
			this.question = question ?? throw new ArgumentNullException(nameof(question));
			this.message = message ?? throw new ArgumentNullException(nameof(message));
			this.uiThread = uiThread ?? throw new ArgumentNullException(nameof(uiThread));
		}

		public Task<bool> AskDropExistingDatabaseAsync(string dbName) {
			var tcs = new TaskCompletionSource<bool>();
			uiThread.Post(async () => {
				try {
					bool answer = await question.QuestionAsync(
						$"База с именем `{dbName}` уже существует на сервере. Удалить существующую базу перед созданием новой?",
						"Создание базы данных");
					tcs.TrySetResult(answer);
				}
				catch(Exception ex) { tcs.TrySetException(ex); }
			});
			return tcs.Task;
		}

		public Task ReportErrorAsync(string text, string lastExecutedStatement) {
			var tcs = new TaskCompletionSource<bool>();
			uiThread.Post(() => {
				try {
					message.ShowMessage(ImportanceLevel.Error, text, "Ошибка создания базы");
					tcs.TrySetResult(true);
				}
				catch(Exception ex) { tcs.TrySetException(ex); }
			});
			return tcs.Task;
		}
	}
}
