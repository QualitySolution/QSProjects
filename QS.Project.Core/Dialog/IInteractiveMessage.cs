using System;
namespace QS.Dialog
{
	/// <summary>
	/// Показывает пользователю интерактивное сообщение.
	/// </summary>
	public interface IInteractiveMessage
	{
		/// <summary>
		/// Показывает сообщение. GUI-реализация должна показать его модально
		/// и вернуть управление только после закрытия сообщения пользователем.
		/// </summary>
		void ShowMessage(ImportanceLevel level, string message, string title = null);
	}

	public enum ImportanceLevel
	{
		Info,
		Warning,
		Error,
		Success
	}
}
