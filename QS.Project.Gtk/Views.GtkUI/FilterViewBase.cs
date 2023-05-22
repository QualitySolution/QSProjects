using System;
using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Dialog;

namespace QS.Views.GtkUI
{
	public class FilterViewBase<TFilter> : Gtk.Bin, ISingleUoWDialog
		where TFilter : FilterViewModelBase<TFilter>
	{
		protected TFilter ViewModel { get; }

		IUnitOfWork ISingleUoWDialog.UoW => ViewModel.UoW;

		public FilterViewBase(TFilter filterViewModel)
		{
			ViewModel = filterViewModel;
		}

		public override void Destroy()
		{
			if(ViewModel != null && ViewModel.DisposeOnDestroy) {
				ViewModel.Dispose();
			}
			base.Destroy();
		}
	}
}
