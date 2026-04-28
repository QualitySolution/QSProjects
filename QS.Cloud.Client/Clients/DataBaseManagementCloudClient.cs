using QS.Cloud.Core;
namespace QS.Cloud.Client.Clients {
	public class DataBaseManagementCloudClient : CloudClientByBasicAuth {
		public DataBaseManagementCloudClient(IBasicAuthInfoProvider basicAuthInfoProvider)
						: base(basicAuthInfoProvider, "core.cloud.qsolution.ru", 443) { }

		public CreateDataBaseResponse CreateDataBase(string dbName, string dbTitle) {
			var client = new DataBaseManagement.DataBaseManagementClient(Channel);
			var request = new CreateDataBaseRequest { Name = dbName, Title = dbTitle };
			return client.CreateDataBase(request, headers); ;
		}
	}
}
