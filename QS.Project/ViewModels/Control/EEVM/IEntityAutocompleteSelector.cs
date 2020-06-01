using System;

namespace QS.ViewModels.Control.EEVM
{
	public interface IEntityAutocompleteSelector<TEntity>
	{
		string GetTitle(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;
		void LoadAutocompletion(string[] searchText, int takeCount);
	}
}
