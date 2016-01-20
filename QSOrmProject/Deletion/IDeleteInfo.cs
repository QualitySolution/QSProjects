﻿using System;
using System.Collections.Generic;

namespace QSOrmProject.Deletion
{
	public interface IDeleteInfo
	{
		Type ObjectClass { get;}
		string ObjectsName { get;}

		List<DeleteDependenceInfo> DeleteItems { get;}
		List<ClearDependenceInfo> ClearItems { get;}

		IList<EntityDTO> GetEntitiesList (DeleteDependenceInfo depend, uint forId);
		IList<EntityDTO> GetEntitiesList(ClearDependenceInfo depend, uint forId);
		EntityDTO GetSelfEntity(uint id);
		Operation CreateDeleteOperation (DeleteDependenceInfo depend, uint forId);
		Operation CreateDeleteOperation (uint selfId);
		Operation CreateClearOperation(ClearDependenceInfo depend, uint forId);
	}

	public interface IDeleteInfoHibernate : IDeleteInfo
	{
		
	}
}

