using System.Threading;

namespace QS.DBScripts.Controllers {
	public abstract class DbCreationResources {
		public string ConnectionString { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
