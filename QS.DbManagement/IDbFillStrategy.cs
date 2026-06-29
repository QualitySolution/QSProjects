using System.Threading;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DbManagement {
	public sealed class DbFillResources {
		public string ConnectionString { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}

	public interface IDbFillStrategy {
		IDbCreatorModel CreateFiller(DbFillResources resources);
	}
}
