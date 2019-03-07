using System;
using Gtk;

namespace QS.Project.Dialogs.GtkUI.ServiceDlg
{
	public class SimpleUserNotificationView : MessageDialog, ISimpleUserNotificationView
	{
		public SimpleUserNotificationView() : base(
				null,
				DialogFlags.Modal,
				MessageType.Other,
				ButtonsType.Ok,
				"")
		{
		}

		public void Notificate(NotificationType type, string text, string caption)
		{
			switch(type) {
				case NotificationType.Info:
					MessageType = MessageType.Info;
					break;
				case NotificationType.Warning:
					MessageType = MessageType.Warning;
					break;
				case NotificationType.Error:
					MessageType = MessageType.Error;
					break;
			}

			SetPosition(WindowPosition.Center);
			Text = text;
			Title = caption;
			ShowAll();

			Run();

			Destroy();
		}
	}
}
