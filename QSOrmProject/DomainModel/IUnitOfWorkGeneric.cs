using System;

namespace QSOrmProject
{
	public interface IUnitOfWorkGeneric<TRootEntity> : IUnitOfWork
		where TRootEntity : IDomainObject, new()
	{
		TRootEntity Root { get;}
	}
}

