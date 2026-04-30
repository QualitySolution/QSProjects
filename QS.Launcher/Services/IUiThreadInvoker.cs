using System;

namespace QS.Launcher.Services {
	/// <summary>
	/// проксирование действия в UI-поток
	/// </summary>
	public interface IUiThreadInvoker {
		/// <summary>Запланировать действие в UI-потоке без его блокировки</summary>
		void Post(Action action);
	}
}
