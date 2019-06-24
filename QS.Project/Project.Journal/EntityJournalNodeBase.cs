using System;
using QS.DomainModel.Entity;

namespace QS.Project.Journal
{
	public class JournalEntityNodeBase : JournalNodeBase
	{
		public Type EntityType { get; }

		public virtual int Id { get; protected set; }

		protected JournalEntityNodeBase(Type entityType)
		{
			EntityType = entityType;
		}
	}

	public class JournalEntityNodeBase<TEntity> : JournalEntityNodeBase
		where TEntity : class, IDomainObject
	{
		public JournalEntityNodeBase() : base(typeof(TEntity))
		{
		}
	}
}
