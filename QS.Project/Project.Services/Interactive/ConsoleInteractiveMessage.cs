using System;
using QS.Dialog;
namespace QS.Project.Services.Interactive
{
	public class ConsoleInteractiveMessage : IInteractiveMessage
	{
		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			Console.WriteLine(message);
		}
	}
}
