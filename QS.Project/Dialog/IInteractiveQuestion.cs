using System;
namespace QS.Dialog
{
	public interface IInteractiveQuestion
	{
		bool Question(string message, string title = null);
	}
}
