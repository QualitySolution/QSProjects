using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Resources;
using System.Threading;

namespace QS.DbManagement.Creation {
	/// <summary>
	/// Наполнение MariaDB базы пользовательским дампом.
	/// Метод блокирует вызывающий поток — выносить в фон ответственность вызывающего кода.
	/// </summary>
	public class MariaDbImportModel : IDbCreatorModel {
		private readonly string connectionString;
		private readonly string dumpFilePath;
		private readonly IProgressBarDisplayable progress;
		private readonly CancellationToken cancellation;

		public MariaDbImportModel(
			DbDumpResources resources) {
			this.connectionString = resources.ConnectionString
				?? throw new ArgumentNullException(nameof(connectionString));
			this.dumpFilePath = resources.DumpFilePath;
			this.progress = resources.Progress;
			this.cancellation = resources.CancellationToken;
		}

		public bool RunCreation(string dbName, string dbTitle) {
			if(string.IsNullOrWhiteSpace(dumpFilePath))
				throw new ArgumentException("Не задан путь к дампу", nameof(dumpFilePath));

			var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
			new MariaDbDumpService().Import(connectionStringBuilder, dbName, dumpFilePath, progress, cancellation, dbTitle);
			cancellation.ThrowIfCancellationRequested();
			return true;
		}
	}
}
