using System;

namespace QSOrmProject
{
	public class BusinessObjectBase<TEntity> : PropertyChangedBase, IBusinessObject 
		where TEntity : IDomainObject, new()
	{
		public IUnitOfWorkGeneric<TEntity> UoWGeneric { set; get;}
		public IUnitOfWork UoW { set; get;}
	}
}

