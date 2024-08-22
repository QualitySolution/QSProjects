using QS.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Cloud.Client
{
	public class SessionManagementCloudClient : CloudClientBySession
	{
		public SessionManagementCloudClient(ISessionInfoProvider sessionInfoProvider, string serviceAddress, int servicePort)
			: base(sessionInfoProvider, serviceAddress, servicePort) { }

		public SessionManagementCloudClient(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "core.cloud.qsolution.ru", 4200) { }

		public bool ChangePassword(string newPassword)
		{
			var client = new SessionManagement.SessionManagementClient(Channel);

			var request = new ChangePasswordRequest
			{
				NewPassword = newPassword
			};

			var response = client.ChangePassword(request, headers);
			return response.Success;
		}

		public bool CloseSession()
		{
			var client = new SessionManagement.SessionManagementClient(Channel);

			var request = new CloseSessionRequest();

			var response = client.CloseSession(request, headers);
			return response.Success;
		}

		public SessionStatuses Alive()
		{
			var client = new SessionManagement.SessionManagementClient(Channel);
			var request = new AliveRequest();

			var response = client.Alive(request, headers);
			return response.ResponseStream.Current.Status;
		}
	}
}
