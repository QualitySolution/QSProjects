using System;
namespace QS.Project.Journal.EntitySelector
{
	public interface IEntityAutocompleteSelectorFactory : IEntitySelectorFactory
	{
		IEntityAutocompleteSelector CreateAutocompleteSelector();
	}
}
