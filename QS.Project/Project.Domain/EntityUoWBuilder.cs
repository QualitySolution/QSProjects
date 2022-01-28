using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
namespace QS.Project.Domain
{
	public interface IEntityUoWBuilder
	{
		bool IsNewEntity { get; }
		IUnitOfWork RootUoW { get; }
		int EntityOpenId { get; }

		IUnitOfWorkGeneric<TEntity> CreateUoW<TEntity>(IUnitOfWorkFactory unitOfWorkFactory, string UoWTitle = null)
			where TEntity : class, IDomainObject, new();
	}

	public class EntityUoWBuilder : IEntityUoWBuilder
	{
		EntityUoWBuilder()
		{
			IsNewEntity = false;
			RootUoW = null;
			EntityOpenId = 0;
		}

		public bool IsNewEntity { get; private set; }
		public int EntityOpenId { get; private set; }
		public IUnitOfWork RootUoW { get; private set; }

		public IUnitOfWorkGeneric<TEntity> CreateUoW<TEntity>(IUnitOfWorkFactory unitOfWorkFactory, string UoWTitle = null) where TEntity : class, IDomainObject, new()
		{
			if(IsNewEntity) {
				if(RootUoW == null) {
					return unitOfWorkFactory.CreateWithNewRoot<TEntity>(UoWTitle);
				} else {
					return unitOfWorkFactory.CreateWithNewChildRoot<TEntity>(RootUoW, UoWTitle);
				}
			} else {
				if(RootUoW == null) {
					return unitOfWorkFactory.CreateForRoot<TEntity>(EntityOpenId, UoWTitle);
				} else {
					return unitOfWorkFactory.CreateForChildRoot<TEntity>(RootUoW.GetById<TEntity>(EntityOpenId), RootUoW, UoWTitle);
				}
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

		/// <summary>
		/// Для создания новой сущности, в дочернем UoW
		/// </summary>
		/// <param name="rootUoW">Родительский UoW</param>
		public static EntityUoWBuilder ForCreateInChildUoW(IUnitOfWork rootUoW) => new EntityUoWBuilder { IsNewEntity = true, RootUoW = rootUoW };

		/// <summary>
		/// Для открытия новой сущности, в дочернем UoW
		/// </summary>
		/// <param name="rootUoW">Родительский UoW</param>
		public static EntityUoWBuilder ForOpenInChildUoW(int entityId, IUnitOfWork rootUoW) => new EntityUoWBuilder { EntityOpenId = entityId, RootUoW = rootUoW };

		#endregion
	}

}