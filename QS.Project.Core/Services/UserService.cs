using QS.Project.Domain;
using System;

namespace QS.Services {
	public class UserService : IUserService
	{
		private readonly UserBase currentUser;

		public UserService(UserBase currentUser)
		{
			this.currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
		}

		public int CurrentUserId => currentUser.Id;

		public UserBase GetCurrentUser() => currentUser;
	}
}
