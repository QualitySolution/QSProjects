using System;
namespace QS.Project.Dialogs
{
	[Obsolete("Этот интерфейс явлется излишним и был нужен на этапе перехода, не используйте его используйте отдельно новые интерфейсы.")]
	public interface IUserNotificationViewAgent
	{
		IRunOperationView GetRunOperationView();
		ISimpleUserNotificationView GetSimpleUserNotificationView();
		ISimpleQuestionUserView GetSimpleUserQuestionView();
	}
}
