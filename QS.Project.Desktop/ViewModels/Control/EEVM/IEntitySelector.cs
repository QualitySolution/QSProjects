using System;
namespace QS.ViewModels.Control.EEVM
{
	public interface IEntitySelector
	{
		void OpenSelector(string dialogTitle = null);
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
	}
}
