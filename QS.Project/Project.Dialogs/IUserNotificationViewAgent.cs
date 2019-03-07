using System;
namespace QS.Project.Dialogs
{
	public interface IUserNotificationViewAgent
	{
		IRunOperationView GetRunOperationView();
		ISimpleUserNotificationView GetSimpleUserNotificationView();
		ISimpleQuestionUserView GetSimpleUserQuestionView();
	}
}
