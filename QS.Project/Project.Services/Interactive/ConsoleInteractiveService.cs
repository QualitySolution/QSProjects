using System;
using QS.Dialog;
using QS.Services;
namespace QS.Project.Services.Interactive
{
	public class ConsoleInteractiveService : IInteractiveService
	{
		public IInteractiveMessage interactiveMessage;
		public IInteractiveMessage InteractiveMessage {
			get {
				if(interactiveMessage == null) {
					interactiveMessage = new ConsoleInteractiveMessage();
				}
				return interactiveMessage;
			}
		}

		public IInteractiveQuestion interactiveQuestion;
		public IInteractiveQuestion InteractiveQuestion {
			get {
				if(interactiveQuestion == null) {
					interactiveQuestion = new ConsoleInteractiveQuestion();
				}
				return interactiveQuestion;
			}
		}
	}
}
