using System;
using QS.DomainModel.UoW;

namespace QS.Dialog.Gtk
{
	public abstract class SingleUowTabBase : TdiTabBase, ISingleUoWDialog, IDisposable
	{
		public SingleUowTabBase()
		{
		}

		public virtual IUnitOfWork UoW { get; protected set; }

		bool canDisposeUoW = false;

		protected void CreateDisposableUoW()
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			canDisposeUoW = true;
		}

		public override void Dispose()
		{
			if(canDisposeUoW)
				UoW?.Dispose();
			base.Dispose();
		}
	}
}
