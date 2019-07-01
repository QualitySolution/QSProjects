using System;
using System.Collections.Generic;
using System.Linq;
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

		public static UserBase GetUserById(IUnitOfWork uow, int id) => uow.GetById<UserBase>(id);

		public static IList<UserBase> GetUsers(IUnitOfWork uow, bool showDeactivated)
		{
			var usersQuery = uow.Session.QueryOver<UserBase>();
			if(!showDeactivated)
				usersQuery.Where(x => !x.Deactivated);
			var result = usersQuery.List()
								   .OrderByDescending(x => x.IsAdmin)
								   .ThenBy(x => x.Name)
								   .Distinct(new UserBaseEqualityComparer())
								   .ToList()
								   ;
			return result;
		}
	}
}
