using System;
using QS.DbManagement;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	/// <summary>
	/// Один шаг пайплайна создания базы
	/// </summary>
	public sealed class DbCreationPhase {
		public string Title { get; }
		public Func<CreatorFactoryArgs, bool> Action { get; }

		public DbCreationPhase(string title, Func<CreatorFactoryArgs, bool> action) {
			Title = title ?? throw new ArgumentNullException(nameof(title));
			Action = action ?? throw new ArgumentNullException(nameof(action));
		}
	}
}
