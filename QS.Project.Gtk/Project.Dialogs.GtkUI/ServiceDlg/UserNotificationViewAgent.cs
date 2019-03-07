using System;
namespace QS.Project.Dialogs.GtkUI.ServiceDlg
{
	public class UserNotificationViewAgent : IUserNotificationViewAgent
	{
		public IRunOperationView GetRunOperationView()
		{
			return new RunOperationView();
		}

		public ISimpleUserNotificationView GetSimpleUserNotificationView()
		{
			return new SimpleUserNotificationView();
		}

		public ISimpleQuestionUserView GetSimpleUserQuestionView()
		{
			return new SimpleQuestionUserView();
		}
	}
}
