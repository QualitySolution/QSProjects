using System;
using QS.DomainModel.UoW;

namespace QS.DomainModel.Tracking
{
	internal class UowLink
	{
		WeakReference uow;

		public UnitOfWorkTitle Title { get; private set; }

		public int SessionHashCode { get; private set; }

		public UowLink(IUnitOfWorkTracked uow)
		{
			SessionHashCode = uow.Session.GetHashCode();
			this.uow = new WeakReference(uow);
			this.Title = uow.ActionTitle;
		}

		public IUnitOfWorkTracked UnitOfWork => uow.Target as IUnitOfWorkTracked;
	}
}
