using System;
using System.Threading;
using QS.Dialog;

namespace QS.DbManagement.Entities {
	/// <summary>
	/// Контекст выполнения одной фазы операциии <see cref="DbCreationPhase"/>
	/// </summary>
	public class DbPhaseArgs {
		public IDbProvider Provider { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
	}
}
