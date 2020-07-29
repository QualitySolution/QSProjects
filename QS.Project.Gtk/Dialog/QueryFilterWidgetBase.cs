using System;
using Gtk;
using QS.DomainModel.UoW;
using QS.Tools;

namespace QS.Dialog.GtkUI
{
	public abstract class QueryFilterWidgetBase : Bin, IQueryFilterView
	{
		public virtual IUnitOfWork UoW { get; set; }

		public event EventHandler Refiltered;

		public abstract IQueryFilter GetQueryFilter();

		public override void Destroy()
		{
			if(UoW != null) {
				UoW.Dispose();
			}
			base.Destroy();
		}

		protected virtual void Refilter()
		{
			Refiltered?.Invoke(this, EventArgs.Empty);
		}
	}
}
