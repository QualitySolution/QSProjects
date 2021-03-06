﻿using System;
using Gtk;

namespace QSProjectsLib
{
	[Obsolete("Используйте аналогичный класс из новых библиотек MessageDialogHelper. Все изменения будут вносится туда. Здесь код оставлен для обратной совместимости.")]
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

		public static void RunErrorDialog(string error)
		{
			RunErrorDialog(true, error);
		}

		public static void RunErrorDialog (bool useMarkup, string error)
		{
			MessageDialog md = new MessageDialog (null,
				DialogFlags.Modal,
				MessageType.Error,
				ButtonsType.Ok,
			    useMarkup,
				error);
			md.SetPosition (WindowPosition.Center);
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

