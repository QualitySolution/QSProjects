using System;
using QS.DomainModel.Entity;

namespace QS.DomainModel.UoW
{
	[Obsolete("Данный интерфейс будет удален в будущем, используйте обычный IUnitOfWork")]
	public interface IUnitOfWorkGeneric<TRootEntity> : IUnitOfWork
		where TRootEntity : IDomainObject, new()
	{
		TRootEntity Root { get;}
	}
}

