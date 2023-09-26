namespace QS.Cloud.Client {
	public class SessionInfoProvider : ISessionInfoProvider {
		public SessionInfoProvider(string sessionId) {
			SessionId = sessionId;
		}

		public string SessionId { get; }
	}

	public interface ISessionInfoProvider {
		string SessionId { get; }
	}
}
