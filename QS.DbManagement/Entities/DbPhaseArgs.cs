using System;
using System.Threading;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DbManagement.Entities {
	/// <summary>
	/// Контекст выполнения одной фазы операции с базой (создание+наполнение, бэкап и т.п.).
	/// Раннер прогресса заполняет его и передаёт в каждую фазу пайплайна.
	/// </summary>
	public class DbPhaseArgs {
		public IDbProvider Provider { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
	}
}
