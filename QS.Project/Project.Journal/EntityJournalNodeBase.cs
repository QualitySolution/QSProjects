using System;
using QS.DomainModel.Entity;

namespace QS.Project.Journal
{
	public class JournalEntityNodeBase : JournalNodeBase
	{
		public Type EntityType { get; set; }

		public virtual int Id { get; set; }

		protected JournalEntityNodeBase(Type entityType)
		{
			EntityType = entityType;
		}

		protected JournalEntityNodeBase() { }
	}

	public class JournalEntityNodeBase<TEntity> : JournalEntityNodeBase
		where TEntity : class, IDomainObject
	{
		public JournalEntityNodeBase() : base(typeof(TEntity))
		{
		}
	}
}
