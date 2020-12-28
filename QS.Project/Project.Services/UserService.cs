﻿using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;

namespace QS.Services
{
	public class UserService : IUserService
	{
		public UserService()
		{
			 CurrentUserId = UserRepository.GetCurrentUserId();
		}
		
		public UserService(int currentUserId)
		{
			CurrentUserId = currentUserId;
		}

		public int CurrentUserId { get; }

		public UserBase GetCurrentUser(IUnitOfWork uow)
		{
			return uow.GetById<UserBase>(CurrentUserId);
		}
	}
}
