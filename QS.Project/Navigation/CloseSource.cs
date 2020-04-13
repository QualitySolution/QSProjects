namespace QS.Navigation
{
	public enum CloseSource
	{
		/// <summary>
		/// Вызов закрытия из внешнего кода обычно вызов xxxxClosePage() через NavigationManager
		/// </summary>
		External,
		/// <summary>
		/// Закрытия ViewModel из собственного кода
		/// </summary>
		Self,
		/// <summary>
		/// Закрытие ViewModel в результате сохранения
		/// </summary>
		Save,
		/// <summary>
		/// Закрытие ViewModel в результате отмены сохранения пользователем, обычно кнопка "Отмена" 
		/// </summary>
		Cancel,
		/// <summary>
		/// Закрытие ViewModel в результате нажатия крестик в заголовке окна или вкладки ViewModel-и
		/// </summary>
		ClosePage,
		/// <summary>
		/// Закрытие дочерней ViewModel из родительской ViewModel
		/// </summary>
		FromParentPage,
		/// <summary>
		/// Закрытие ViewModel вызванное закрытием родительской ViewModel
		/// </summary>
		WithParentPage,
		/// <summary>
		/// Закрытие подчиненной ViewModel вызванное закрытием master ViewModel
		/// </summary>
		WithMasterPage,
		/// <summary>
		/// Закрытие ViewModel вызванное выходом из приложения
		/// </summary>
		AppQuit,
	}
}
