using System;

namespace QS.DomainModel.UoW
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

		public bool CanCheckIfDirty { get; set; }

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

