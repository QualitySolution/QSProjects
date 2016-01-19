using System;
using System.Collections.Generic;

namespace QSOrmProject.Deletion
{
	public interface IDeleteInfo
	{
		Type ObjectClass { get;}

		List<DeleteDependenceInfo> DeleteItems { get;}
		List<ClearDependenceInfo> ClearItems { get;}
	}
}

