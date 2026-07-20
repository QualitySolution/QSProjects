using QS.Dialog;
using System.Threading;

namespace QS.DBScripts.Controllers {
	public abstract class DbCreationResources {
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interactions { get; set; }
		public string ConnectionString { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
