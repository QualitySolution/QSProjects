using Grpc.Core;
using QS.Cloud.Core;
using QS.Project.Versioning;
using System.Threading;

namespace QS.Cloud.Client.Clients {
	public class DataBaseManagementCloudClient : CloudClientByBasicAuth {
		private readonly uint ProductCode;
		public DataBaseManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider, uint productCode)
						: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 443)
		{
			ProductCode = productCode;
		}
		public virtual ClearDataBaseResponse ClearDataBase(int baseId) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new ClearDataBaseRequest { BaseId = baseId, ProductId = ProductCode };
			return client.ClearDataBase(request, headers);
		}

		public virtual CheckDataBaseExistsResponse CheckDataBaseExists(string dbName) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new CheckDataBaseExistsRequest { Name = dbName, ProductId = ProductCode };
			return client.CheckDataBaseExists(request, headers);
		}

		public virtual CreateDataBaseResponse CreateDataBase(string dbName, string dbTitle) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new CreateDataBaseRequest { Name = dbName, Title = dbTitle, ProductId = ProductCode };
			return client.CreateDataBase(request, headers);
		}

		public virtual DropDataBaseResponse DropDataBase(int baseId) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new DropDataBaseRequest { BaseId = baseId, ProductId = ProductCode };
			return client.DropDataBase(request, headers);
		}
	}
}
