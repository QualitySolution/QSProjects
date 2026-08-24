using MySqlConnector;
using QS.DbManagement.Entities;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherMetadataManagement {
		public const string LauncherBaseName = "QSLauncher";

		private readonly string connectionString;
		public LauncherBasesManagement Bases { get; }
		public LauncherUsersManagement Users { get; }

		public LauncherMetadataManagement(MySqlConnectionStringBuilder connectionBuilder, bool canSync, string login, byte productId)
		{
			Bases = new LauncherBasesManagement(connectionBuilder, canSync, productId);
			Users = new LauncherUsersManagement(connectionBuilder, login, productId, Bases);

			// строку правим на копии: builder принадлежит вызывающему
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName
			};
			connectionString = toLauncher.ConnectionString;
		}

		public int CreateBaseWithCreatorAccess(DbInfo dbInfo) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				using(var transaction = connection.BeginTransaction()) {
					int baseId = Bases.InsertBase(connection, transaction, dbInfo);
					Users.GrantCreatorUpdateRight(connection, transaction, baseId);
					transaction.Commit();
					return baseId;
				}
			}
		}
	}
}
