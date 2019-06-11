using System;
using Gtk;
using QS.DomainModel.UoW;

namespace QS.Dialog.GtkUI
{
	public class SingleUoWWidgetBase : Bin, ISingleUoWDialog
	{
		public SingleUoWWidgetBase()
		{
		}

		public IUnitOfWork UoW { get; set; }

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}
