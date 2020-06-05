using System;
namespace QS.ViewModels.Control.EEVM
{
	public interface IEntityAdapter<TEntity>
		where TEntity : class
	{
		TEntity GetEntityByNode(object node);
		EntityEntryViewModel<TEntity> EntityEntryViewModel { set; }
	}
}
