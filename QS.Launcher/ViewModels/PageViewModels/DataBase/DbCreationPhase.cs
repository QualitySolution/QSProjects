using System;
using System.Threading.Tasks;
using QS.DbManagement;
using QS.DBScripts.Controllers;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	/// <summary>
	/// Один шаг пайплайна создания базы. Settings-VM собирает список таких фаз
	/// и передаёт его в Progress-VM, который выполняет их последовательно.
	/// </summary>
	public sealed class DbCreationPhase {
		public string Title { get; }
		public Func<CreatorFactoryArgs, Task<bool>> Action { get; }

		public DbCreationPhase(string title, Func<CreatorFactoryArgs, Task<bool>> action) {
			Title = title ?? throw new ArgumentNullException(nameof(title));
			Action = action ?? throw new ArgumentNullException(nameof(action));
		}
	}
}
