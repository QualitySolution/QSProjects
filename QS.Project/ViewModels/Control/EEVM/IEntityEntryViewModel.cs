using System;
using System.Collections;
using System.ComponentModel;

namespace QS.ViewModels.Control.EEVM
{
	public interface IEntityEntryViewModel : INotifyPropertyChanged
	{
		string EntityTitle { get; }
		object Entity { get; set; }

		bool SensetiveSelectButton { get; }
		bool SensetiveCleanButton { get; }
		bool SensetiveAutoCompleteEntry { get; }
		bool SensetiveViewButton { get; }

		void OpenSelectDialog();
		void CleanEntity();
		void OpenViewEntity();

		int AutocompleteListSize { get; set; }
		void AutocompleteTextEdited(string text);
		string GetAutocompleteTitle(object node);
		void SetEntityByNode(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;
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
