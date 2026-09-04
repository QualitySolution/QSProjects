using System;

namespace QS.DbManagement.Entities {
	/// <summary>
	/// Один шаг пайплайна длительной операции с базами - создания, импорта, синхронизации
	/// </summary>
	public sealed class DbCreationPhase {
		public string Title { get; }
		public Func<DbPhaseArgs, bool> Action { get; }

		public DbCreationPhase(string title, Func<DbPhaseArgs, bool> action) {
			Title = title ?? throw new ArgumentNullException(nameof(title));
			Action = action ?? throw new ArgumentNullException(nameof(action));
		}
	}
}
