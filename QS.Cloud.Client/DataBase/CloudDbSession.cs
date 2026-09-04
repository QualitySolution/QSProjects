using System;
using MySqlConnector;
using QS.Cloud.Client.Clients;
using QS.Cloud.Core;

namespace QS.Cloud.Client.DataBase {
	/// <summary>
	/// Временное подключение к облачной базе
	/// </summary>
	public sealed class CloudDbSession : IDisposable {
		private readonly AliveCloudClient sessionLife;

		public bool Success { get; }
		public string Description { get; }
		public bool IsAdmin { get; }
		public BaseConnection Db { get; }
		public MySqlConnectionStringBuilder ConnectionStringBuilder { get; }

		private CloudDbSession(StartSessionResponse session, AliveCloudClient sessionLife, MySqlConnectionStringBuilder connectionStringBuilder) {
			Success = session.Success;
			Description = session.Description;
			IsAdmin = session.IsAdmin;
			Db = session.Db;
			this.sessionLife = sessionLife;
			ConnectionStringBuilder = connectionStringBuilder;
		}

		/// <summary>
		/// Открывает сессию к базе. При успехе запускает keep-alive и собирает строку подключения;
		/// при отказе возвращает сессию с <see cref="Success"/> = false
		/// </summary>
		public static CloudDbSession Open(LoginManagementCloudClient loginClient, int baseId) {
			var session = loginClient.StartSession(baseId);
			if(!session.Success)
				return new CloudDbSession(session, null, null);

			var sessionLife = new AliveCloudClient(new SessionInfoProvider(session.SessionId));
			sessionLife.KeepAlive();

			var builder = new MySqlConnectionStringBuilder {
				Server = session.Db.Server,
				Port = session.Db.Port,
				UserID = session.Db.Login,
				Password = session.Db.Password,
				Database = session.Db.BaseName,
				AllowUserVariables = true
			};
			return new CloudDbSession(session, sessionLife, builder);
		}

		public void Dispose() => sessionLife?.Dispose();
	}
}
