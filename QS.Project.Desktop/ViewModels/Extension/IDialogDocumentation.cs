namespace QS.ViewModels.Extension {
	/// <summary>
	/// Если ViewModel диалога реализует этот интерфейс, на вкладке будет отображаться кнопка для открытия документации.
	/// </summary>
	public interface IDialogDocumentation {
		string DocumentationUrl { get; }
		string ButtonTooltip { get; }
	}
}
