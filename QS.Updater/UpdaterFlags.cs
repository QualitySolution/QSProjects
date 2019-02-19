using System;

namespace QS.Updater
{
	[Flags]
	public enum UpdaterFlags
	{
		/// <summary>
		/// Запуск с параметрами по умолчанию.
		/// </summary>
		None = 0,
		/// <summary>
		/// Показать диалог вне зависимости от результата проверки.
		/// </summary>
		ShowAnyway = 1,
		/// <summary>
		/// Запустить диалог в фоновом потоке.
		/// </summary>
		StartInThread = 2,
		/// <summary>
		/// Не продолжать работу без обновления.
		/// </summary>
		UpdateRequired = 4
	}
}