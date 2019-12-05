using System;
namespace QS.Project.Journal.EntitySelection
{
	public interface IEntitySelector
	{
		void OpenSelector(string dialogTitle = null);
		object RefreshEntity(object entity);
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
	}

	public interface IEntityAutocompleteSelector
	{

	}
}
