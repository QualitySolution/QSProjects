using System;
namespace QS.ViewModels.Control.EEVM
{
	public class FuncEntityAdapter<TEntity> : IEntityAdapter<TEntity>
		where TEntity : class
	{
		private readonly Func<object, TEntity> getEntityByNode;

		public FuncEntityAdapter(Func<object, TEntity> getEntityByNode)
		{
			this.getEntityByNode = getEntityByNode ?? throw new ArgumentNullException(nameof(getEntityByNode));
		}

		public EntityEntryViewModel<TEntity> EntityEntryViewModel { set; get; }

		public TEntity GetEntityByNode(object node)
		{
			return getEntityByNode(node);
		}
	}
}
