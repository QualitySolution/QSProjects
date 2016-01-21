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

		IList<EntityDTO> GetDependEntities (DeleteCore core, DeleteDependenceInfo depend, EntityDTO masterEntity);
		IList<EntityDTO> GetDependEntities (DeleteCore core, ClearDependenceInfo depend, EntityDTO masterEntity);
		EntityDTO GetSelfEntity(DeleteCore core, uint id);
		Operation CreateDeleteOperation (DeleteDependenceInfo depend, uint forId);
		Operation CreateDeleteOperation (uint selfId);
		Operation CreateClearOperation(ClearDependenceInfo depend, uint forId);
	}

	public interface IDeleteInfoHibernate : IDeleteInfo
	{
		
	}
}

