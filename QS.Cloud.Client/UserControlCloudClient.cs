using QS.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Cloud.Client
{
	public class UserControlCloudClient : CloudClientServiceBase
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

			var response = client.CreateUser(request, Headers);

			return response.Success;
		}

		public bool DeleteUser(string login)
		{
			var client = new UserControl.UserControlClient(Channel);

			var request = new DeleteUserRequest { User = login };
			var response = client.DeleteUser(request);

			return response.Success;
		}


	}
}
