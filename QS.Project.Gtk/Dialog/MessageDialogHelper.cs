using System;
using Gtk;

namespace QS.Dialog.GtkUI
{
	public static class MessageDialogHelper
	{
		public static bool RunQuestionDialog (string question, params object[] args)
		{
			return RunQuestionDialog (String.Format (question, args));
		}

		public static bool RunQuestionDialog (string question)
		{
			MessageDialog md = new MessageDialog (null,
				                   DialogFlags.Modal,
				                   MessageType.Question,
				                   ButtonsType.YesNo,
				                   question);
			md.SetPosition (WindowPosition.Center);
			md.ShowAll ();
			bool result = md.Run () == (int)ResponseType.Yes;
			md.Destroy ();
			return result;
		}

		public static bool RunQuestionWithTitleDialog(string title, string question)
		{
			MessageDialog md = new MessageDialog(null,
								   DialogFlags.Modal,
								   MessageType.Question,
								   ButtonsType.YesNo,
								   question);
			md.SetPosition(WindowPosition.Center);
			md.Title = title;
			md.ShowAll();
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}

		public static void RunWarningDialog (string warning, string title = null)
		{
			MessageDialog md = new MessageDialog (null,
				                   DialogFlags.Modal,
				                   MessageType.Warning,
				                   ButtonsType.Ok,
				                   warning);
			md.SetPosition (WindowPosition.Center);
			md.Title = title ?? "Предупреждение";
			md.ShowAll ();
			md.Run ();
			md.Destroy ();
		}

		public static bool RunWarningDialog(string title, string warning, ButtonsType buttons = ButtonsType.YesNo)
		{
			MessageDialog md = new MessageDialog(null,
								   DialogFlags.Modal,
								   MessageType.Warning,
								   buttons,
								   warning);
			md.SetPosition(WindowPosition.Center);
			md.Title = title;
			md.ShowAll();
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}

		public static void RunErrorDialog (string formattedError, params object[] args){
			RunErrorDialog(String.Format(formattedError, args));
		}

		public static void RunErrorDialog(string error, string title = null)
		{
			RunErrorDialog(true, error, title);
		}

		public static void RunErrorDialog (bool useMarkup, string error, string title = null)
		{
			MessageDialog md = new MessageDialog (null,
				DialogFlags.Modal,
				MessageType.Error,
				ButtonsType.Ok,
			    useMarkup,
				error);
			md.SetPosition (WindowPosition.Center);
			md.Title = title ?? "Ошибка";
			md.ShowAll ();
			md.Run ();
			md.Destroy ();
		}

		public static void RunErrorWithSecondaryTextDialog(string error, string secondaryText)
		{
			MessageDialog md = new MessageDialog(null,
				DialogFlags.Modal,
				MessageType.Error,
				ButtonsType.Ok,
				error);
			md.SetPosition(WindowPosition.Center);
			if(!String.IsNullOrEmpty(secondaryText))
				md.SecondaryText = secondaryText;
			md.ShowAll();
			md.Run();
			md.Destroy();
		}

		public static void RunInfoDialog(string formattedMessage, params object[] args)
		{
			RunInfoDialog(String.Format(formattedMessage, args));
		}

		public static void RunInfoDialog (string message, string title = null)
		{
			MessageDialog md = new MessageDialog (null,
				DialogFlags.Modal,
				MessageType.Info,
				ButtonsType.Ok,
				message);

			var messageLabel = GetMessageLabel(md);
			if(messageLabel != null)
            {
				messageLabel.LineWrap = false;
			}

			md.SetPosition (WindowPosition.Center);
			md.Title = title ?? "Информация";
			md.ShowAll ();
			md.Run ();
			md.Destroy ();
		}

		private static Label GetMessageLabel(MessageDialog messageDialog)
        {
			return ((messageDialog.VBox.Children[0] as HBox)?.Children[1] as VBox)?.Children[0] as Label;
		}
	}
}

