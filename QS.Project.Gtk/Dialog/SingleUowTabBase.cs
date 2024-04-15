using System;
using QS.DomainModel.UoW;

namespace QS.Dialog.Gtk
{
	public abstract class SingleUowTabBase : TdiTabBase, ISingleUoWDialog
	{
		public SingleUowTabBase()
		{
		}

		public virtual IUnitOfWork UoW { get; protected set; }
	}
}
