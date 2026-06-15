using Grpc.Core;
using QS.Cloud.Core;
using QS.Project.Versioning;
using System.Threading;

namespace QS.Cloud.Client.Clients {
	public class DataBaseManagementCloudClient : CloudClientByBasicAuth {
		private IApplicationInfo applicationInfo { get; set; }
		public DataBaseManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider)
						: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 443) 
		{
			this.applicationInfo = applicationInfo;
		}

		public CreateDataBaseResponse CreateDataBase(string dbName, string dbTitle, IApplicationInfo applicationInfo) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new CreateDataBaseRequest { Name = dbName, Title = dbTitle, ProductId = applicationInfo.ProductCode };
			return client.CreateDataBase(request, headers);
		}

		public DropDataBaseResponse DropDataBase(int baseId) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new DropDataBaseRequest { BaseId = baseId };
			return client.DropDataBase(request, headers);
		}
	}
}
