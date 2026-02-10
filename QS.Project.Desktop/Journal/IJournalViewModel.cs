using System.Collections;
using QS.Journal.Actions;
using QS.Journal.Search;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;

namespace QS.Journal {
	/// <summary>
	/// Интерфейс view model журнала для взаимодействия с view
	/// </summary>
	public interface IJournalViewModel : IDialogViewModel {
		/// <summary>
		/// Фильтр журнала
		/// </summary>
		IJournalFilterViewModel JournalFilter { get; }
		
		/// <summary>
		/// Показывать ли фильтр
		/// </summary>
		bool IsFilterShow { get; set; }
		
		/// <summary>
		/// Поиск в журнале
		/// </summary>
		IJournalSearch Search { get; }
		
		/// <summary>
		/// Включен ли поиск
		/// </summary>
		bool SearchEnabled { get; }
		
		/// <summary>
		/// Раскрывать ли дерево после перезагрузки
		/// </summary>
		bool ExpandAfterReloading { get; set; }
		
		/// <summary>
		/// Загрузчик данных журнала
		/// </summary>
		IDataLoader DataLoader { get; }
		
		/// <summary>
		/// Элементы журнала
		/// </summary>
		IList Items { get; }
		
		/// <summary>
		/// Информация для футера (строка состояния)
		/// </summary>
		string FooterInfo { get; }
		
		/// <summary>
		/// View model для панели действий журнала
		/// </summary>
		IJournalEventsHandler ActionsViewModel { get; }
		
		/// <summary>
		/// Режим выбора строк в таблице журнала
		/// </summary>
		JournalSelectionMode TableSelectionMode { get; set; }
		
		/// <summary>
		/// Режим выбора для журнала (влияет на наличие кнопки "Выбрать")
		/// </summary>
		JournalSelectionMode SelectionMode { get; set; }
		
		/// <summary>
		/// Popup действия (контекстное меню)
		/// </summary>
		System.Collections.Generic.IEnumerable<IJournalAction> PopupActions { get; }
		
		/// <summary>
		/// Событие выбора элементов в журнале
		/// </summary>
		event System.EventHandler<JournalSelectedEventArgs> OnSelectResult;
		
		/// <summary>
		/// Обновить данные журнала
		/// </summary>
		void Refresh(bool needResetItemsCountForNextLoad = true);
	}
}
