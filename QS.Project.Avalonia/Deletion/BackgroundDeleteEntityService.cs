using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System;
using System.Threading.Tasks;

namespace QS.Deletion;

public class BackgroundDeleteEntityService(IDeleteEntityService deletion, IGuiDispatcher guiDispatcher) : IDeleteEntityService {
	public DeleteCore DeleteEntity<TEntity>(int id, IUnitOfWork? uow = null, Action? beforeDeletion = null, bool forceDelete = false) =>
		Run(() => deletion.DeleteEntity<TEntity>(id, uow, beforeDeletion, forceDelete));

	public DeleteCore DeleteEntity(Type clazz, int id, IUnitOfWork? uow = null, Action? beforeDeletion = null, bool forceDelete = false) =>
		Run(() => deletion.DeleteEntity(clazz, id, uow, beforeDeletion, forceDelete));

	private DeleteCore Run(Func<DeleteCore> deleting) {
		var running = Task.Run(deleting);
		guiDispatcher.WaitInMainLoop(() => running.IsCompleted);
		return running.GetAwaiter().GetResult();
	}
}
