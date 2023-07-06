using System;
using QS.DomainModel.Entity;

namespace QS.Project.Journal
{
	public abstract class JournalEntityNodeBase : IJournalNode 
	{
		public Type EntityType { get; set; }

		public virtual int Id { get; set; }
		
		public abstract string Title { get; }

		protected JournalEntityNodeBase(Type entityType) {
			EntityType = entityType;
		}

		protected JournalEntityNodeBase() { }
	}

	public abstract class JournalEntityNodeBase<TEntity> : JournalEntityNodeBase
		where TEntity : class, IDomainObject
	{
		public JournalEntityNodeBase() : base(typeof(TEntity))
		{
		}
	}
}
