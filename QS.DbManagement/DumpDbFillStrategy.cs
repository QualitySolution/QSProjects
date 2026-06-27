using System;
using MySqlConnector;
using QS.DBScripts.Controllers;

namespace QS.DbManagement {
	public class DumpDbFillStrategy : IDbFillStrategy {
		private readonly string dumpFilePath;

		public DumpDbFillStrategy(string dumpFilePath) {
			if(string.IsNullOrWhiteSpace(dumpFilePath))
				throw new ArgumentException("Не задан путь к дампу", nameof(dumpFilePath));
			this.dumpFilePath = dumpFilePath;
		}

		public IDbCreatorModel CreateFiller(DbFillResources resources) {
			return new MariaDbImportModel(
				new MySqlConnectionStringBuilder(resources.ConnectionString),
				dumpFilePath,
				resources.Progress,
				resources.CancellationToken);
		}
	}
}
