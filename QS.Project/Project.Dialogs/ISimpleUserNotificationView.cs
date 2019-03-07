using System;

namespace QS.Project.Dialogs
{
	public interface ISimpleUserNotificationView
	{
		void Notificate(NotificationType type, string text, string caption);
	}

	public enum NotificationType
	{
		Info,
		Warning,
		Error
	}
}
