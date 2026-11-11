using System;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Launcher.Updates {
	public interface ILauncherUpdateService {
		event Action UpdatesDisabled;
		Task<LauncherUpdateResult> CheckAsync(CancellationToken cancellationToken);
		void ShowUpdate();
	}

	public sealed class LauncherUpdateResult {
		public string Version { get; set; }
		public bool IsSkipped { get; set; }
	}
}
