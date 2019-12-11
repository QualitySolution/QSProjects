using System;
namespace QS.ViewModels.Control.EEVM
{
	public interface IEntitySelector
	{
		void OpenSelector(string dialogTitle = null);
		object RefreshEntity(object entity);
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
	}
}
