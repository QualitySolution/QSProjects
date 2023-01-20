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
		
		public string Question(string[] buttons, string message, string title = null)
		{
			Console.WriteLine(message);
			for(int i = 0; i < buttons.Length; i++) {
				Console.WriteLine($@"{i+1} - {buttons[i]}");
			}
			Console.Write("Введите номер ответа: ");
			string answer = Console.ReadLine();
			if(Int32.TryParse(answer, out int number)) {
				if(number > 0 && buttons.Length >= number) {
					return buttons[number + 1];
				}
			}
			return null;
		}
	}
}
