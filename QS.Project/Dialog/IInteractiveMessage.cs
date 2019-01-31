using System;
namespace QS.Dialog
{
	public interface IInteractiveMessage
	{
		void ShowMessage(ImportanceLevel level, string message);
	}

	public enum ImportanceLevel
	{
		Info,
		Warning,
		Error
	}
}
