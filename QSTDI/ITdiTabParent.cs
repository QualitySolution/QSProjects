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

		/// <summary>
		/// Выполняем первую стадию создания новой вкладки с диалогом.
		/// </summary>
		/// <returns>Флаги ответов.</returns>
		/// <param name="subject">Объект для которого будет открыта вкладка, для поиска такой же открытой.</param>
		/// <param name="masterTab">Если не null, то вкладка будет являтся подчиненной.</param>
		/// <param name="CanSlided">Указываем можно ли добавлять в слайдер или обязательно открывать новую вкладку.</param>
		TdiBeforeCreateResultFlag BeforeCreateNewTab(object subject, ITdiTab masterTab, bool CanSlided = true);

		/// <summary>
		/// Выполняем первую стадию создания новой вкладки с журналом.
		/// </summary>
		/// <returns>Флаги ответов.</returns>
		/// <param name="subjectType">Тип объекта журнала, для поиска уже открытой вкладки.</param>
		/// <param name="masterTab">Если не null, то вкладка будет являтся подчиненной.</param>
		/// <param name="CanSlided">Указываем можно ли добавлять в слайдер или обязательно открывать новую вкладку.</param>
		TdiBeforeCreateResultFlag BeforeCreateNewTab(System.Type subjectType, ITdiTab masterTab, bool CanSlided = true);

		ITdiTab FindTab(string hashName);

		void SwitchOnTab(ITdiTab tab);
	}

	[Flags]
	public enum TdiBeforeCreateResultFlag
	{
		Ok = 0x00,
		Canceled = 0x01,
		AlreadyExist = 0x02
	}
}

