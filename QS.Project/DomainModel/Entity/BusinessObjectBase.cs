using System;
using QS.DomainModel.UoW;

namespace QS.DomainModel.Entity
{
	public abstract class BusinessObjectBase<TEntity> : PropertyChangedBase, IBusinessObject 
		where TEntity : IDomainObject, new()
	{
		public virtual IUnitOfWork UoW { set; get;}
	}
}

