using System;
using QS.Dialog;
using System.Linq;
namespace QS.Project.Services.Interactive
{
	public class ConsoleInteractiveQuestion : IInteractiveQuestion
	{
		public bool Question(string message, string title = null)
		{
			string[] yesOptions = { "да", "д", "yes", "y" };
			string[] noOptions = { "нет", "н", "no", "n" };
			Console.WriteLine($"{message} (да/нет)");
			while(true) {
				string answer = Console.ReadLine();
				if(yesOptions.Contains(answer)) {
					return true;
				}
				if(noOptions.Contains(answer)) {
					return false;
				}
			}
		}
	}
}
