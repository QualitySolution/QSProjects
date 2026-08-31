using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherMetadataManagement {
		public const string LauncherBaseName = "QSLauncher";

		private readonly string connectionString;
		public LauncherBasesManagement Bases { get; }
		public LauncherUsersManagement Users { get; }

		public LauncherMetadataManagement(MySqlConnectionStringBuilder connectionBuilder, bool isServerAdmin,
			string login, byte productId)
		{
			Bases = new LauncherBasesManagement(connectionBuilder, isServerAdmin, productId);
			Users = new LauncherUsersManagement(connectionBuilder, login, isServerAdmin, Bases);

			// строку правим на копии: builder принадлежит вызывающему
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName
			};
			connectionString = toLauncher.ConnectionString;

			EnsureAvailable();
		}

		private void EnsureAvailable() {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				connection.ExecuteScalar<int?>("SELECT 1 FROM `bases` LIMIT 1;");
			}
		}

		public int CreateBaseWithCreatorAccess(DbInfo dbInfo) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				using(var transaction = connection.BeginTransaction()) {
					int baseId = Bases.UpsertBase(connection, transaction, dbInfo);
					Users.GrantCreatorUpdateRight(connection, transaction, baseId);
					transaction.Commit();
					return baseId;
				}
			}
		}
	}
}
