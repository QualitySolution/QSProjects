using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Core;

namespace QS.Cloud.Client
{
	public class UserManagementCloudClient : CloudClientByBasicAuth
	{
		public UserManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider)
			: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 443) { }

		public List<UserInfo> GetUsers()
		{
			var client = new UserManagement.UserManagementClient(Channel);
			var response = client.GetUsers(new GetUsersRequest(), headers);
			return response.Users.ToList();
		}

		public CreateUserResponse CreateUser(UserInfo user, string password)
		{
			var client = new UserManagement.UserManagementClient(Channel);
			var request = new CreateUserRequest {
				Login = user.Login,
				Name = user.Name ?? "",
				Email = user.Email ?? "",
				Password = password ?? "",
				Phone = user.Phone ?? "",
				Post = user.Post ?? "",
				Comment = user.Comment ?? "",
				IsAccountAdmin = user.IsAccountAdmin
			};
			return client.CreateUser(request, headers);
		}

		public UpdateUserResponse UpdateUser(UserInfo user, string newPassword)
		{
			var client = new UserManagement.UserManagementClient(Channel);
			var request = new UpdateUserRequest {
				Login = user.Login,
				Name = user.Name ?? "",
				Email = user.Email ?? "",
				Phone = user.Phone ?? "",
				Post = user.Post ?? "",
				Comment = user.Comment ?? "",
				Disabled = user.Disabled,
				IsAccountAdmin = user.IsAccountAdmin,
				NewPassword = newPassword ?? ""
			};
			return client.UpdateUser(request, headers);
		}

		public DeleteUserResponse DeleteUser(string login)
		{
			var client = new UserManagement.UserManagementClient(Channel);
			var request = new DeleteUserRequest { User = login };
			return client.DeleteUser(request, headers);
		}

		public List<BaseAccessInfo> GetUserBaseAccess(string login, uint productId)
		{
			var client = new UserManagement.UserManagementClient(Channel);
			var request = new GetUserBaseAccessRequest { User = login, ProductId = productId };
			var response = client.GetUserBaseAccess(request, headers);
			return response.Bases.ToList();
		}

		public bool ChangeBaseAccess(string user, int baseId, bool grant, bool admin, bool readOnly, uint productId)
		{
			var client = new UserManagement.UserManagementClient(Channel);
			var request = new ChangeBaseAccessRequest {
				User = user,
				BaseId = baseId,
				Grant = grant,
				Admin = admin,
				ReadOnly = readOnly,
				ProductId = productId
			};
			var response = client.ChangeBaseAccess(request, headers);
			return response.Success;
		}
	}
}
