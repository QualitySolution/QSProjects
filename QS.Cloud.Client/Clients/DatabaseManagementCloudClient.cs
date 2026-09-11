using Grpc.Core;
using QS.Cloud.Core;
using QS.Project.Versioning;
using System.Threading;

namespace QS.Cloud.Client.Clients {
	public class DatabaseManagementCloudClient : CloudClientByBasicAuth {
		private readonly uint ProductCode;
		public DatabaseManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider, uint productCode)
						: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 443)
		{
			ProductCode = productCode;
		}
		public virtual ClearDatabaseResponse ClearDatabase(int baseId) {
			var client = new DatabaseManagement.DatabaseManagementClient(Channel);
			var request = new ClearDatabaseRequest { BaseId = baseId, ProductId = ProductCode };
			return client.ClearDatabase(request, headers);
		}

		public virtual CheckDatabaseExistsResponse CheckDatabaseExists(string dbName) {
			var client = new DatabaseManagement.DatabaseManagementClient(Channel);
			var request = new CheckDatabaseExistsRequest { Name = dbName, ProductId = ProductCode };
			return client.CheckDatabaseExists(request, headers);
		}

		public virtual CreateDatabaseResponse CreateDatabase(string dbName, string dbTitle) {
			var client = new DatabaseManagement.DatabaseManagementClient(Channel);
			var request = new CreateDatabaseRequest { Name = dbName, Title = dbTitle, ProductId = ProductCode };
			return client.CreateDatabase(request, headers);
		}

		public virtual DropDatabaseResponse DropDatabase(int baseId) {
			var client = new DatabaseManagement.DatabaseManagementClient(Channel);
			var request = new DropDatabaseRequest { BaseId = baseId, ProductId = ProductCode };
			return client.DropDatabase(request, headers);
		}
	}
}
