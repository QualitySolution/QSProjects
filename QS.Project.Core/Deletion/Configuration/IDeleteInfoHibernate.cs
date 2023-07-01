using System;

namespace QS.Deletion.Configuration
{
	internal interface IDeleteInfoHibernate : IDeleteInfo
	{
		bool IsRootForSubclasses { get; }
		bool IsRequiredCascadeDeletion { get; }
		bool IsSubclass { get; }
		Type[] GetSubclasses();
		Type GetRootClass();
	}
}