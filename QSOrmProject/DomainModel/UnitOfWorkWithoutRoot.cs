using System;
using QS.DomainModel.Tracking;
using QSOrmProject;

namespace QS.DomainModel
{
	public class UnitOfWorkWithoutRoot : UnitOfWorkBase, IUnitOfWork
	{
		public object RootObject {
			get { return null;}
		}

		public bool HasChanges
		{
			get
			{
				return Session.IsDirty();
			}
		}

		internal UnitOfWorkWithoutRoot(UnitOfWorkTitle title)
		{
			IsNew = false;
            ActionTitle = title;
		}

		public void Save()
		{
			throw new InvalidOperationException ("В этой реализации UoW отсутствует Root, для завершения транзакции используйте метод Commit()");
		}
	}
}

