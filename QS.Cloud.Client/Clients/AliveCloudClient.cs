using Grpc.Core;
using QS.Cloud.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Cloud.Client
{
	public class AliveCloudClient : CloudClientBySession {
		public AliveCloudClient(ISessionInfoProvider sessionInfoProvider, string serviceAddress, int servicePort)
			: base(sessionInfoProvider, serviceAddress, servicePort) { }
		public AliveCloudClient(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, "core.cloud.qsolution.ru", 4200) { }

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public event Action<SessionStatuses> StatusChanged;

		public event Action<string> NewMessage;

		public SessionStatuses LastStatus { get; protected set; }

		private DateTime? failSince;

		private async Task Listen() {
			var client = new SessionManagement.SessionManagementClient(Channel);
			var request = new AliveRequest();

			var call = client.Alive(request, headers);
			while (await call.ResponseStream.MoveNext(new CancellationToken())) {
				failSince = null;

				var curStatus = call.ResponseStream.Current.Status;
				if (curStatus != LastStatus) {
					string message = call.ResponseStream.Current.Message;
					LastStatus = curStatus;

					logger.Debug("Сообщение потока: " + message);

					StatusChanged?.Invoke(curStatus);
					NewMessage?.Invoke(message);
				}
			}
		}

		private void FaultedTaskHandling(Task task) {
			if(task.IsCanceled || (task.Exception?.InnerException as RpcException)?.StatusCode == StatusCode.Cancelled)
				logger.Info("Соединение с QS.Cloud отменено");

			else if(task.IsFaulted) {
				if(failSince == null)
					failSince = DateTime.Now;

				var failedTime = (DateTime.Now - failSince).Value;
				if(failedTime.Seconds < 10)
					Thread.Sleep(1000);
				else if(failedTime.Minutes < 10)
					Thread.Sleep(4000);
				else
					Thread.Sleep(30000);

				logger.Error(task.Exception);

				logger.Info($"Соединение с QS.Cloud разорвано... Пробуем соединиться.");
				ConnectionLoop(new CancellationToken());
			}
		}

		private void ConnectionLoop(CancellationToken token) {
			var readerTask = 
				Task.Run(Listen, token)
				.ContinueWith(FaultedTaskHandling);
		}

		public void KeepAlive() {
			ConnectionLoop(new CancellationToken());
		}
	}
}
