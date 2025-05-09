using QS.Dialog;

namespace QS.Updater
{
	public readonly struct UpdateInfo 
	{
		public string Title { get; }
		
		public string Message { get; }
		
		public UpdateStatus Status { get; }
		
		public ImportanceLevel ImportanceLevel { get; }
		
		public UpdateInfo(string title, string message, UpdateStatus status, ImportanceLevel importanceLevel) 
		{
			Title = title;
			Message = message;
			Status = status;
			ImportanceLevel = importanceLevel;
		}
	}
	
	public enum UpdateStatus 
	{
		Ok,
		/// <summary>
		/// Обновление пропущено пользователем
		/// </summary>
		Skip,
		/// <summary>
		/// Обновление отложено пользователем
		/// </summary>
		Shelve,
		/// <summary>
		/// Запущена установка обновления. Необходимо закрыть приложение.
		/// </summary>
		AppUpdateIsRunning,
		UpToDate,
		/// <summary>
		/// Ошибка версии базы данных
		/// </summary>
		BaseError,
		/// <summary>
		/// Дополнительные сообщения со стороны сервера.
		/// </summary>
		ExternalError,
		/// <summary>
		/// Ошибка подключения к серверу обновлений.
		/// </summary>
		ConnectionError
	}
}
