using System;
using Gtk;

namespace QS.Dialog.GtkUI
{
	public class GtkQuestionDialogsInteractive : IInteractiveQuestion
	{
		public bool Question(string message, string title = null)
		{
			MessageDialog md = new MessageDialog(null,
								   DialogFlags.Modal,
								   MessageType.Question,
								   ButtonsType.YesNo,
								   message);
			md.SetPosition(WindowPosition.Center);
			md.Title = title;
			md.ShowAll();
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}
	}
}
