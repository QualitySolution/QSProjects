using QS.Dialog;
using System.Threading.Tasks;

namespace QS.Launcher {
	public static class InteractiveQuestionExtensions {
		public static Task<bool> AskInBackground(this IInteractiveQuestion question, string message, string title = null)
			=> Task.Run(() => question.Question(message, title));
	}
}
