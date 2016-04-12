using System;

namespace QSTDI
{
	public interface ITdiTabParent
	{
		void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab, bool CanSlided = true);
		void AddTab(ITdiTab tab, ITdiTab afterTab, bool CanSlided = true);

		/// <summary>
		/// Выполняем проверку есть ли у вкладки подчиненные, если есть выводится сообщение пользователю с переключение на незакрытую подчинённую вкладку.
		/// </summary>
		/// <returns><c>true</c>, если подчиненных вкладки есть, если все готово для закрытия текущей <c>false</c>.</returns>
		/// <param name="tab">Вкладка для проверки.</param>
		bool CheckClosingSlaveTabs(ITdiTab tab);

		ITdiTab FindTab(string hashName);

		void SwitchOnTab(ITdiTab tab);

		ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null);
	}
}

