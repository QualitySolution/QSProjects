using System;
namespace QS.Dialog.GtkUI
{
	public class GtkMessageDialogsInteractive : IInteractiveMessage
	{
		public void ShowMessage(ImportanceLevel level, string message)
		{
			switch(level) {
				case ImportanceLevel.Error:
					MessageDialogHelper.RunErrorDialog(message);
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
