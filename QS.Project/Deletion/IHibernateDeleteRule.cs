using System;

namespace QS.Deletion
{
	public interface IHibernateDeleteRule : IDeleteRule
	{
		bool IsRootForSubclasses { get; }
		bool IsRequiredCascadeDeletion { get; }
		bool IsSubclass { get; }
		Type[] GetSubclasses();
	}
}