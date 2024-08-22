using QS.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Cloud.Client
{
	public class UserControlCloudClient : CloudClientBySession
	{
		public UserControlCloudClient(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "core.cloud.qsolution.ru", 4200) { }


		public bool CreateUser(string login, string userName, string email, string password)
		{
			var client = new UserControl.UserControlClient(Channel);

			var request = new CreateUserRequest
			{
				Login = login, Name = userName, Email = email, Password = password
			};

			var response = client.CreateUser(request, headers);

			return response.Success;
		}

		public bool DeleteUser(string login)
		{
			var client = new UserControl.UserControlClient(Channel);

			var request = new DeleteUserRequest { User = login };
			var response = client.DeleteUser(request, headers);

			return response.Success;
		}

		// strange, but protobuf has the same signature
		public bool UpdateUser()
		{
			var client = new UserControl.UserControlClient(Channel);

			var request = new UpdateUserRequest();
			var response = client.UpdateUser(request, headers);

			return response.Success;
		}

		public bool ChangeBaseAccess(string user, int baseId, bool grant, bool admin)
		{
			var client = new UserControl.UserControlClient(Channel);

			var request = new ChangeBaseAccessRequest
			{
				User = user,
				BaseId = baseId,
				Grant = grant,
				Admin = admin
			};

			var response = client.ChangeBaseAccess(request);

			return response.Success;
		}
	}
}
