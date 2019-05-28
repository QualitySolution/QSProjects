using System;
namespace QS.Dialog
{
	public interface IInteractiveMessage
	{
		void ShowMessage(ImportanceLevel level, string message, string title = null);
	}

	public enum ImportanceLevel
	{
		Info,
		Warning,
		Error
	}
}
