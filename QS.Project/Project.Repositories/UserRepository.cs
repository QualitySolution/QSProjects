using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;

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

		public static UserBase GetUserById(IUnitOfWork uow, int id)
		{
			return uow.GetById<UserBase>(id);
		}

		public static IList<UserBase> GetUsers(IUnitOfWork uow, bool showDeactivated)
		{
			var usersQuery = uow.Session.QueryOver<UserBase>();
			if(!showDeactivated) {
				usersQuery.Where(Restrictions.Eq(Projections.Property<UserBase>(x => x.Deactivated), false));
			}
			return usersQuery.List();
		}
	}
}
