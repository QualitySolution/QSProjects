using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Core;

namespace QS.Cloud.Client
{
	public class LoginManagementCloudClient : CloudClientByBasicAuth
	{
		public LoginManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider)
			: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 4200) { }

		public StartResponse Start(string launcherVersion) {
			var client = new LoginManagement.LoginManagementClient(Channel);
			var request = new StartRequest { LauncherVersion = launcherVersion };
			var response = client.Start(request, headers);
			return response;
		}

		public StartSessionResponse StartSession(int baseId)
		{
			var client = new LoginManagement.LoginManagementClient(Channel);

			var request = new StartSessionRequest
			{
				BaseId = baseId
			};
			var response = client.StartSession(request, headers);
			
			return response;
		}

		public List<BaseInfo> GetBasesForUser()
		{
			var client = new LoginManagement.LoginManagementClient(Channel);
			var request = new GetBasesForUserRequest();
			var response = client.GetBasesForUser(request, headers);
			return response.Bases.ToList();
		}

		public ChangePasswordResponse ChangePassword(string newPassword) {
			var client = new LoginManagement.LoginManagementClient(Channel);
			var request = new ChangePasswordRequest { NewPassword = newPassword };
			var response = client.ChangePassword(request, headers);
			return response;
		}
	}
}
