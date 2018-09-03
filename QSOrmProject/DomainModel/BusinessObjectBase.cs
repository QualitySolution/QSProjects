using System;

namespace QSOrmProject
{
	public abstract class BusinessObjectBase<TEntity> : PropertyChangedBase, IBusinessObject 
		where TEntity : IDomainObject, new()
	{
		public virtual IUnitOfWorkGeneric<TEntity> UoWGeneric { set; get;}
		public virtual IUnitOfWork UoW { set; get;}
	}
}

