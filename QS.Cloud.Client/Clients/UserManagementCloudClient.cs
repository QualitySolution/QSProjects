using QS.Cloud.Core;

namespace QS.Cloud.Client
{
	public class UserManagementCloudClient : CloudClientBySession
	{
		public UserManagementCloudClient(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "core.cloud.qsolution.ru", 4200) { }


		public CreateUserResponse CreateUser(string login, string userName, string email, string password)
		{
			var client = new UserManagement.UserManagementClient(Channel);

			var request = new CreateUserRequest
			{
				Login = login, Name = userName, Email = email, Password = password
			};

			var response = client.CreateUser(request, headers);

			return response;
		}

		public DeleteUserResponse DeleteUser(string login)
		{
			var client = new UserManagement.UserManagementClient(Channel);

			var request = new DeleteUserRequest { User = login };
			var response = client.DeleteUser(request, headers);

			return response;
		}

		// strange, but protobuf has the same signature
		public UpdateUserResponse UpdateUser()
		{
			var client = new UserManagement.UserManagementClient(Channel);

			var request = new UpdateUserRequest();
			var response = client.UpdateUser(request, headers);

			return response;
		}

		public bool ChangeBaseAccess(string user, int baseId, bool grant, bool admin)
		{
			var client = new UserManagement.UserManagementClient(Channel);

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
