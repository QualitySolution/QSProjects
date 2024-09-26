using QS.Cloud.Core;
using System;
using System.Threading.Tasks;

namespace QS.Cloud.Client.Clients
{
	public class AliveCloudClient : CloudClientBySession {
		public AliveCloudClient(ISessionInfoProvider sessionInfoProvider, string serviceAddress, int servicePort)
			: base(sessionInfoProvider, serviceAddress, servicePort) { }
		public AliveCloudClient(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "core.cloud.qsolution.ru", 4200) { }

		public event Action<SessionStatuses> StatusChanged;

		public SessionStatuses LastStatus { get; protected set; }

		public async Task Listen() {
			var client = new SessionManagement.SessionManagementClient(Channel);
			var request = new AliveRequest();

			var call = client.Alive(request, headers);
			while (await call.ResponseStream.MoveNext(new System.Threading.CancellationToken())) {
				var curStatus = call.ResponseStream.Current.Status;
				if (curStatus != LastStatus) {
					LastStatus = curStatus;
					StatusChanged?.Invoke(curStatus);
				}
			}
		}
	}
}
