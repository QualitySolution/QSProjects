using System;
using QS.DomainModel.UoW;
namespace QS.Project.Domain
{
	[Obsolete("[Только для Водовоза]")]
	public interface IEntityConstructorParam
	{
		/// <summary>
		/// Необходимо создание новой сущности
		/// </summary>
		bool IsNewEntity { get; }

		/// <summary>
		/// Id существующей сущности
		/// </summary>
		int EntityOpenId { get; }

		/// <summary>
		/// Родительский UoW
		/// </summary>
		IUnitOfWork RootUoW { get; }
	}

	[Obsolete("[Только для Водовоза]Не используйте этот класс для передачи параметров создания UnitOfWork, используйте вместо этого EntityUoWBuilder.")]
	public class EntityConstructorParam : IEntityConstructorParam
	{
		EntityConstructorParam()
		{
			IsNewEntity = false;
			RootUoW = null;
			EntityOpenId = 0;
		}

		public bool IsNewEntity { get; private set; }
		public int EntityOpenId { get; private set; }
		public IUnitOfWork RootUoW { get; private set; }

		/// <summary>
		/// Для создания новой сущности
		/// </summary>
		public static EntityConstructorParam ForCreate() => new EntityConstructorParam { IsNewEntity = true };

		/// <summary>
		/// Для открытия новой сущности
		/// </summary>
		public static EntityConstructorParam ForOpen(int entityId) => new EntityConstructorParam { EntityOpenId = entityId };

		/// <summary>
		/// Для создания новой сущности, в дочернем UoW
		/// </summary>
		/// <param name="rootUoW">Родительский UoW</param>
		public static EntityConstructorParam ForCreateInChildUoW(IUnitOfWork rootUoW) => new EntityConstructorParam { IsNewEntity = true, RootUoW = rootUoW };

		/// <summary>
		/// Для открытия новой сущности, в дочернем UoW
		/// </summary>
		/// <param name="rootUoW">Родительский UoW</param>
		public static EntityConstructorParam ForOpenInChildUoW(int entityId, IUnitOfWork rootUoW) => new EntityConstructorParam { EntityOpenId = entityId, RootUoW = rootUoW };
	}
}