using QS.DomainModel.Tracking;
using QS.Project.DB;
using System;
using System.Threading.Tasks;

namespace QS.DomainModel.UoW {
	public class UnitOfWorkWithoutRoot : UnitOfWorkBase, IUnitOfWork
	{
		public object RootObject => null;

		public bool HasChanges => Session.IsDirtyNoFlush();

		internal UnitOfWorkWithoutRoot(ISessionProvider sessionProvider, UnitOfWorkTitle title, SingleUowEventsTracker tracker = null) 
			: base(sessionProvider, tracker)
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

