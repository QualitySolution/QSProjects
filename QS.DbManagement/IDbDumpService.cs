using QS.Dialog;
using System.Threading;

namespace QS.DbManagement {
	public interface IDbDumpService {
		void Export(string connectionString, string databaseName, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation);

		void Import(string connectionString, string databaseName, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation, string title = null);
	}
}
