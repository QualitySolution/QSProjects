using System;
using System.Collections.Generic;

namespace QSOrmProject.Deletion
{

	public interface IDeleteInfoHibernate : IDeleteInfo
	{
		bool IsRootForSubclasses { get; }
		bool IsRequiredCascadeDeletion { get; }
		bool IsSubclass { get; }
		Type[] GetSubclasses();
		IDeleteInfo GetRootDeleteInfo();
	}
}