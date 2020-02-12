using System;
using System.Collections;
using System.ComponentModel;
using QS.ViewModels.Control.EEVM;

namespace QS.ViewModels.Control.ESVM
{
	public interface IEntitySearchViewModel : INotifyPropertyChanged
	{

		bool SensetiveCleanButton { get; }
		bool SensetiveAutoCompleteEntry { get; }

		void CleanEntity();

		int AutocompleteListSize { get; set; }
		void AutocompleteTextEdited(string text);
		void SelectNode(object node);
		string GetAutocompleteTitle(object node);
		string SearchText { get; set; }

		event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
	}


}
