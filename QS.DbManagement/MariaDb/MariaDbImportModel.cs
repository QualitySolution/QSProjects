using System;
using System.Threading;
using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DbManagement {
	/// <summary>
	/// Наполнение MariaDB базы пользовательским дампом.
	/// Метод блокирует вызывающий поток — выносить в фон ответственность вызывающего кода.
	/// </summary>
	public class MariaDbImportModel : IDbCreatorModel {
		private readonly MySqlConnectionStringBuilder connectionStringBuilder;
		private readonly string dumpFilePath;
		private readonly IProgressBarDisplayable progress;
		private readonly CancellationToken cancellation;

		public MariaDbImportModel(
			MySqlConnectionStringBuilder connectionStringBuilder,
			string dumpFilePath,
			IProgressBarDisplayable progress,
			CancellationToken cancellation) {
			this.connectionStringBuilder = connectionStringBuilder
				?? throw new ArgumentNullException(nameof(connectionStringBuilder));
			this.dumpFilePath = dumpFilePath;
			this.progress = progress;
			this.cancellation = cancellation;
		}

		public bool RunCreation(string dbName, string dbTitle) {
			new MariaDbDumpService().Import(connectionStringBuilder, dbName, dumpFilePath, progress, cancellation, dbTitle);
			cancellation.ThrowIfCancellationRequested();
			return true;
		}
	}
}
