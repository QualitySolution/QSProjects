using System;
namespace QS.ViewModels.Control.EEVM
{
	public interface IEntityAdapter<TEntity>
	{
		TEntity GetEntityByNode(object node);
	}
}
