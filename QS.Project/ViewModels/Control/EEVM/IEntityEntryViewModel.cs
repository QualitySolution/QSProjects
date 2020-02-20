using System;
using System.Collections;
using System.ComponentModel;

namespace QS.ViewModels.Control.EEVM
{
	public interface IEntityEntryViewModel : INotifyPropertyChanged
	{
		#region Выбранная сущьность
		string EntityTitle { get; }
		object Entity { get; set; }
		#endregion

		#region События для внешних подписчиков
		event EventHandler Changed;
		event EventHandler ChangedByUser;
		#endregion

		#region Доступность функций View
		bool SensetiveSelectButton { get; }
		bool SensetiveCleanButton { get; }
		bool SensetiveAutoCompleteEntry { get; }
		bool SensetiveViewButton { get; }
		#endregion

		#region Команды от View
		void OpenSelectDialog();
		void CleanEntity();
		void OpenViewEntity();
		#endregion

		#region Автодополнение
		int AutocompleteListSize { get; set; }
		void AutocompleteTextEdited(string text);
		string GetAutocompleteTitle(object node);
		void SetEntityByNode(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;
		#endregion
	}

	public class AutocompleteUpdatedEventArgs : EventArgs
	{
		public IList List;

		public AutocompleteUpdatedEventArgs(IList list)
		{
			this.List = list ?? throw new ArgumentNullException(nameof(list));
		}
	}
}
