using System;
using System.Collections.Generic;

namespace QS.Deletion
{
	internal interface IDeleteInfoHibernate : IDeleteInfo
	{
		bool IsRootForSubclasses { get; }
		bool IsRequiredCascadeDeletion { get; }
		bool IsSubclass { get; }
		Type[] GetSubclasses();
		IDeleteInfo GetRootDeleteInfo();
	}
}