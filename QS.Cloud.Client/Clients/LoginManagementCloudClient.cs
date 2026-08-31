using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Core;

namespace QS.Cloud.Client
{
	public class LoginManagementCloudClient : CloudClientByBasicAuth
	{
		public LoginManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider)
                        : base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 443) { }

		public virtual StartResponse Start(string launcherVersion) {
			var client = new LoginManagement.LoginManagementClient(Channel);
			var request = new StartRequest { LauncherVersion = launcherVersion };
			var response = client.Start(request, headers);
			return response;
		}

		public virtual StartSessionResponse StartSession(int baseId)
		{
			var client = new LoginManagement.LoginManagementClient(Channel);

			var request = new StartSessionRequest
			{
				BaseId = baseId
			};
			var response = client.StartSession(request, headers);
			
			return response;
		}

		public virtual List<BaseInfo> GetBasesForUser(uint productId)
		{
			var client = new LoginManagement.LoginManagementClient(Channel);
			var request = new GetBasesForUserRequest();
			request.ProductId = productId;
			var response = client.GetBasesForUser(request, headers);
			return response.Bases.ToList();
		}

		public virtual ChangePasswordResponse ChangePassword(string newPassword) {
			var client = new LoginManagement.LoginManagementClient(Channel);
			var request = new ChangePasswordRequest { NewPassword = newPassword };
			var response = client.ChangePassword(request, headers);
			return response;
		}
	}
}
