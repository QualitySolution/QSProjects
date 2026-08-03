using MySqlConnector;
using QS.DbManagement.Entities;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherMetadataManagement {
		public const string LauncherBaseName = "QSLauncher";

		private readonly string connectionString;
		public LauncherBasesManagement Bases { get; }
		public LauncherUsersManagement Users { get; }

		public LauncherMetadataManagement(MySqlConnectionStringBuilder connectionBuilder, bool canWrite, string login, int productId) {
			// пользователь ищется один раз и отдаёт аккаунт менеджеру баз
			Users = new LauncherUsersManagement(connectionBuilder, login, productId);
			Bases = new LauncherBasesManagement(connectionBuilder, canWrite, Users.CurrentAccountId, productId);

			connectionBuilder.Database = LauncherBaseName;
			connectionString = connectionBuilder.ConnectionString;
		}

		public (int baseId, string baseGuid) CreateBaseWithCreatorAccess(DbInfo dbInfo) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				using(var transaction = connection.BeginTransaction()) {
					var result = Bases.InsertBase(connection, transaction, dbInfo);
					Users.GrantCreatorAccess(connection, transaction, result.baseId);
					transaction.Commit();
					return result;
				}
			}
		}
	}
}
