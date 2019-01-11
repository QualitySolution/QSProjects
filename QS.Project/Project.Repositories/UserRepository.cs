using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.Project.Repositories
{
	public static class UserRepository
	{
		//FIXME Временный проброс ID пользователя, до тех пор пока вход пользователя не переведем с QSProjectsLib на QS.Project
		public static Func<int> GetCurrentUserId;

		public static UserBase GetCurrentUser(IUnitOfWork uow)
		{
			if(GetCurrentUserId == null)
				return null;

			return uow.GetById<UserBase>(GetCurrentUserId());
		}
	}
}
