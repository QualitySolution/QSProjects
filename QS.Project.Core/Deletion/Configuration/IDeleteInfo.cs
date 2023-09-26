using System;
using System.Collections.Generic;

namespace QS.Deletion.Configuration
{
	internal interface IDeleteInfo : IDeleteRule
	{
		string ObjectsName { get;}
		string TableName { get; }

		bool HasDependences { get;}

		IList<EntityDTO> GetDependEntities (IDeleteCore core, DeleteDependenceInfo depend, EntityDTO masterEntity);
		IList<EntityDTO> GetDependEntities (IDeleteCore core, ClearDependenceInfo depend, EntityDTO masterEntity);
		IList<EntityDTO> GetDependEntities (IDeleteCore core, RemoveFromDependenceInfo depend, EntityDTO masterEntity);
		EntityDTO GetSelfEntity(IDeleteCore core, uint id);
		Operation CreateDeleteOperation (EntityDTO masterEntity, DeleteDependenceInfo depend, IList<EntityDTO> dependEntities);
		Operation CreateRemoveFromOperation (EntityDTO masterEntity, RemoveFromDependenceInfo depend, IList<EntityDTO> dependEntities);
		Operation CreateDeleteOperation (EntityDTO entity);
		Operation CreateClearOperation(EntityDTO masterEntity, ClearDependenceInfo depend, IList<EntityDTO> dependEntities);
		Operation CreateUpdateOperation(EntityDTO entityDTO, UpdateDependenceInfo updateDependenceInfo);
	}

	public interface IDeleteRule
	{
		Type ObjectClass { get; }

		List<DeleteDependenceInfo> DeleteItems { get; }
		List<ClearDependenceInfo> ClearItems { get; }
		List<RemoveFromDependenceInfo> RemoveFromItems { get; }
		List<UpdateDependenceInfo> UpdateItems { get; }
	}
}
