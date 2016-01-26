using System;
using System.Collections.Generic;

namespace QSOrmProject.Deletion
{
	public interface IDeleteInfo
	{
		Type ObjectClass { get;}
		string ObjectsName { get;}

		List<DeleteDependenceInfo> DeleteItems { get;}
		List<ClearDependenceInfo> ClearItems { get;}
		List<RemoveFromDependenceInfo> RemoveFromItems { get;}

		IList<EntityDTO> GetDependEntities (DeleteCore core, DeleteDependenceInfo depend, EntityDTO masterEntity);
		IList<EntityDTO> GetDependEntities (DeleteCore core, ClearDependenceInfo depend, EntityDTO masterEntity);
		IList<EntityDTO> GetDependEntities (DeleteCore core, RemoveFromDependenceInfo depend, EntityDTO masterEntity);
		EntityDTO GetSelfEntity(DeleteCore core, uint id);
		Operation CreateDeleteOperation (EntityDTO masterEntity, DeleteDependenceInfo depend, IList<EntityDTO> dependEntities);
		Operation CreateRemoveFromOperation (EntityDTO masterEntity, RemoveFromDependenceInfo depend, IList<EntityDTO> dependEntities);
		Operation CreateDeleteOperation (EntityDTO entity);
		Operation CreateClearOperation(EntityDTO masterEntity, ClearDependenceInfo depend, IList<EntityDTO> dependEntities);
	}

	public interface IDeleteInfoHibernate : IDeleteInfo
	{
		
	}
}

