using QS.Project.Domain;
using QSOrmProject;

namespace QS.Project.Repositories
{
	public static class UserRepository
	{
		public static UserBase GetCurrentUser(IUnitOfWork uow)
		{
			return uow.GetById<UserBase>(OrmMain.CurrentUserId);
		}
	}
}
