using System;

namespace QSOrmProject
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

		public UnitOfWorkWithoutRoot()
		{
			IsNew = false;
		}

		public void Save()
		{
			throw new InvalidOperationException ("В этой реализации UoW отсутствует Root, для завершения транзакции используйте метод Commit()");
		}
	}
}

