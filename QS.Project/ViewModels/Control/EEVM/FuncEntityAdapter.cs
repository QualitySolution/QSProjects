using System;
namespace QS.ViewModels.Control.EEVM
{
	public class FuncEntityAdapter<TEntity> : IEntityAdapter<TEntity>
	{
		private readonly Func<object, TEntity> getEntityByNode;

		public FuncEntityAdapter(Func<object, TEntity> getEntityByNode)
		{
			this.getEntityByNode = getEntityByNode ?? throw new ArgumentNullException(nameof(getEntityByNode));
		}

		public TEntity GetEntityByNode(object node)
		{
			return getEntityByNode(node);
		}
	}
}
