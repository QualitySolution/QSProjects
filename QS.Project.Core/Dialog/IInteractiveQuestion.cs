namespace QS.Dialog
{
	public interface IInteractiveQuestion
	{
		/// <summary>
		/// Отобразит диалог с вопросом пользователю и кнопками Да Нет.
		/// </summary>
		/// <param name="message">Сообщение диалога</param>
		/// <param name="title">Заголовок окна диалога</param>
		/// <returns>True - Да, False - Нет(или закрытие крестиком)</returns>
		bool Question(string message, string title = null);
		
		/// <summary>
		/// Отобразит диалог с вопросом пользователю.
		/// </summary>
		/// <param name="buttons">Список заголовков для кнопок</param>
		/// <param name="message">Сообщение диалога</param>
		/// <param name="title">Заголовок окна диалога</param>
		/// <returns>Вернет заголовок кнопки которую нажал пользователь. Если пользователь закроет диалог крестиком, вернется null.</returns>
		string Question(string[] buttons, string message, string title = null);
	}
}
