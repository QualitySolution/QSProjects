using System;
using Gtk;
namespace QS.Project.Dialogs.GtkUI.ServiceDlg
{
	public class SimpleQuestionUserView : MessageDialog, ISimpleQuestionUserView
	{
		public SimpleQuestionUserView() : base(
			null, 
			DialogFlags.Modal,
			MessageType.Question,
			ButtonsType.YesNo, 
			"")
		{
		}

		public bool RunQuestionView(string text, string title)
		{
			SetPosition(WindowPosition.Center);
			Text = text;
			Title = title;
			ShowAll();

			bool result = Run() == (int)ResponseType.Yes;
			Destroy();
			return result;
		}
	}
}
