using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Project.Domain
{
	public interface IEntityUoWBuilder
	{
		bool IsNewEntity { get; }
		
		int EntityOpenId { get; }

		TEntity GetEntity<TEntity>(IUnitOfWork unitOfWork)
			where TEntity : class, IDomainObject, new();
	}

	public class EntityUoWBuilder : IEntityUoWBuilder
	{
		EntityUoWBuilder()
		{
			IsNewEntity = false;
			EntityOpenId = 0;
		}

		public bool IsNewEntity { get; private set; }
		public int EntityOpenId { get; private set; }

		public TEntity GetEntity<TEntity>(IUnitOfWork unitOfWork) where TEntity : class, IDomainObject, new()
		{
			if(IsNewEntity) {
				return new TEntity();
			} else {
				return unitOfWork.GetById<TEntity>(EntityOpenId);
			}
		}

		#region Статические методы конструкторы

		/// <summary>
		/// Для создания новой сущности
		/// </summary>
		public static EntityUoWBuilder ForCreate() => new EntityUoWBuilder { IsNewEntity = true };

		/// <summary>
		/// Для открытия новой сущности
		/// </summary>
		public static EntityUoWBuilder ForOpen(int entityId) => new EntityUoWBuilder { EntityOpenId = entityId };
		
		#endregion
	}
}
