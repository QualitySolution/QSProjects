using QS.DomainModel.Entity;

namespace QS.DomainModel.UoW
{
	public interface IUnitOfWorkGeneric<TRootEntity> : IUnitOfWork
		where TRootEntity : IDomainObject, new()
	{
		TRootEntity Root { get;}
	}

	public interface IChildUnitOfWorkGeneric<TParentEntity, TChildEntity> : IUnitOfWorkGeneric<TChildEntity> 
		where TChildEntity : IDomainObject, new()
		where TParentEntity : IDomainObject, new()
	{
		
	}
}

