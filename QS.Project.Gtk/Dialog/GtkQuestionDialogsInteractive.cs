using System;
using System.Collections.Generic;
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
			md.Title = title ?? "Вопрос";
			md.ShowAll();
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}

		public string Question(string[] buttons, string message, string title = null) {
			MessageDialog md = new MessageDialog(null,
				DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.None,
				message);
			md.SetPosition(WindowPosition.Center);
			md.Title = title ?? "Вопрос";
			for(int i = 0; i < buttons.Length; i++) {
				md.AddButton(buttons[i], i);
			}
			md.ShowAll();
			int result = md.Run();
			md.Destroy();
			return result >= 0 ? buttons[result] : null;
		}
	}
}
