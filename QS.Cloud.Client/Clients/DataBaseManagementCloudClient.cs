using Grpc.Core;
using QS.Cloud.Core;
using QS.Project.Versioning;
using System.Threading;

namespace QS.Cloud.Client.Clients {
	public class DataBaseManagementCloudClient : CloudClientByBasicAuth {
		public DataBaseManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider)
						: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 443)
		{
		}

		public CreateDataBaseResponse CreateDataBase(string dbName, string dbTitle, IApplicationInfo applicationInfo) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new CreateDataBaseRequest { Name = dbName, Title = dbTitle, ProductId = applicationInfo.ProductCode };
			return client.CreateDataBase(request, headers);
		}

		public DropDataBaseResponse DropDataBase(int baseId, IApplicationInfo applicationInfo) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new DropDataBaseRequest { BaseId = baseId, ProductId = applicationInfo.ProductCode };
			return client.DropDataBase(request, headers);
		}
	}
}
