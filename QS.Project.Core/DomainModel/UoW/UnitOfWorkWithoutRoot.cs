using System;
using System.Threading.Tasks;
using QS.Dialog;
using QS.Project.DB;

namespace QS.DomainModel.UoW
{
	public class UnitOfWorkWithoutRoot : UnitOfWorkBase, IUnitOfWork
	{
		public object RootObject => null;

		public bool HasChanges => Session.IsDirtyNoFlush();

		internal UnitOfWorkWithoutRoot(ISessionProvider sessionProvider, IMainThreadDispatcher mainThreadDispatcher, UnitOfWorkTitle title) 
			: base(sessionProvider, mainThreadDispatcher)
		{
			IsNew = false;
            ActionTitle = title;
		}

		public void Save()
		{
			throw new InvalidOperationException ("В этой реализации UoW отсутствует Root, для завершения транзакции используйте метод Commit()");
		}

		public Task SaveAsync() {
			throw new InvalidOperationException ("В этой реализации UoW отсутствует Root, для завершения транзакции используйте метод Commit()");
		}
	}
}

