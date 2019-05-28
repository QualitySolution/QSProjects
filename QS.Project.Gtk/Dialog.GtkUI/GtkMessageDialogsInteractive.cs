using System;
namespace QS.Dialog.GtkUI
{
	public class GtkMessageDialogsInteractive : IInteractiveMessage
	{
		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			switch(level) {
				case ImportanceLevel.Error:
					MessageDialogHelper.RunErrorDialog(message, title);
					break;
				case ImportanceLevel.Warning:
					MessageDialogHelper.RunWarningDialog(message);
					break;
				case ImportanceLevel.Info:
				default:
					MessageDialogHelper.RunInfoDialog(message);
					break;
			}
		}
	}
}
