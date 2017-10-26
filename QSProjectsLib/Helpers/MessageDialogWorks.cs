using System;
using Gtk;

namespace QSProjectsLib
{
	public static class MessageDialogWorks
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

		public static void RunWarningDialog (string warning)
		{
			MessageDialog md = new MessageDialog (null,
				                   DialogFlags.Modal,
				                   MessageType.Warning,
				                   ButtonsType.Ok,
				                   warning);
			md.SetPosition (WindowPosition.Center);
			md.ShowAll ();
			md.Run ();
			md.Destroy ();
		}

		public static void RunErrorDialog (string formattedError, params object[] args){
			RunErrorDialog(String.Format(formattedError, args));
		}

		public static void RunErrorDialog (string error, string secondaryText = null)
		{
			MessageDialog md = new MessageDialog (null,
				DialogFlags.Modal,
				MessageType.Error,
				ButtonsType.Ok,
				error);
			md.SetPosition (WindowPosition.Center);
			if (!String.IsNullOrEmpty (secondaryText))
				md.SecondaryText = secondaryText;
			md.ShowAll ();
			md.Run ();
			md.Destroy ();
		}

		public static void RunInfoDialog (string message)
		{
			MessageDialog md = new MessageDialog (null,
				DialogFlags.Modal,
				MessageType.Info,
				ButtonsType.Ok,
				message);
			md.SetPosition (WindowPosition.Center);
			md.ShowAll ();
			md.Run ();
			md.Destroy ();
		}
	}
}

