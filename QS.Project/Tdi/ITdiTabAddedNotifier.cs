using System;

namespace QS.Tdi
{
	/// <summary>
	/// Tdi tab added notifier.
	/// Интерфейс для выполнения действия по добавлению вкладки
	/// </summary>
	public interface ITdiTabAddedNotifier : ITdiTab
	{
		void OnTabAdded();
	}
}
