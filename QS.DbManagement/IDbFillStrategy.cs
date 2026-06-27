using System.Threading;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DbManagement {
	/// <summary>
	/// Ресурсы наполнения, известные только в момент операции
	/// </summary>
	public sealed class DbFillResources {
		public string ConnectionString { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}

	public interface IDbFillStrategy {
		IDbCreatorModel CreateFiller(DbFillResources resources);
	}
}
