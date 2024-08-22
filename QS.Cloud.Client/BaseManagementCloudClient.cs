using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Core;

namespace QS.Cloud.Client
{
	public class BaseManagementCloudClient : CloudClientByBasicAuth
	{
		public BaseManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider)
			: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 4200) { }


		public LoginResponse Login(int baseId)
		{
			var client = new BaseManagement.BaseManagementClient(Channel);

			var request = new LoginRequest
			{
				BaseId = baseId
			};
			var response = client.Login(request, headers);
			
			return response;
		}

		public List<BaseInfo> GetBasesForUser()
		{
			var client = new BaseManagement.BaseManagementClient(Channel);
			var request = new GetBasesForUserRequest();
			var response = client.GetBasesForUser(request, headers);
			return response.Bases.ToList();
		}
	}
}
