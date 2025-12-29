using System.ComponentModel;

namespace QS.ViewModels.Extension {
	/// <summary>
	/// Позволяет модели управлять отображением кнопок "Сохранить" и "Отмена" в диалогах.
	/// </summary>
	public interface ISaveCancelManagement : INotifyPropertyChanged {
		bool SaveButtonVisible { get; }
		string CancelButtonLabel { get; }
	}
}
