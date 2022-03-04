using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
namespace QS.Project.Domain
{
	public interface IEntityUoWBuilder
	{
		bool IsNewEntity { get; }
		
		int EntityOpenId { get; }

		IUnitOfWorkGeneric<TEntity> CreateUoW<TEntity>(IUnitOfWorkFactory unitOfWorkFactory, string UoWTitle = null)
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

		public IUnitOfWorkGeneric<TEntity> CreateUoW<TEntity>(IUnitOfWorkFactory unitOfWorkFactory, string UoWTitle = null) where TEntity : class, IDomainObject, new()
		{
			if(IsNewEntity) {
				return unitOfWorkFactory.CreateWithNewRoot<TEntity>(UoWTitle);
			} else {
				return unitOfWorkFactory.CreateForRoot<TEntity>(EntityOpenId, UoWTitle);
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