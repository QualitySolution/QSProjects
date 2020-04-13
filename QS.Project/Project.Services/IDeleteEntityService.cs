using System;
using QS.Deletion;
using QS.DomainModel.UoW;

namespace QS.Project.Services
{
	public interface IDeleteEntityService
	{
		DeleteCore DeleteEntity<TEntity>(int id, IUnitOfWork uow = null, Action beforeDeletion = null);
		DeleteCore DeleteEntity(Type clazz, int id, IUnitOfWork uow = null, Action beforeDeletion = null);
	}
}
