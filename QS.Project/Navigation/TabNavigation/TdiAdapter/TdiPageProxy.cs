using System;
using System.Collections.Generic;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Navigation.TabNavigation.TdiAdapter
{
	public class TdiPageProxy : ITdiPage, IPage<TdiTabViewModelAdapter>
	{
		private readonly IPage<TdiTabViewModelAdapter> page;

		public TdiPageProxy(IPage<TdiTabViewModelAdapter> page)
		{
			this.page = page ?? throw new ArgumentNullException(nameof(page));
			page.PageClosing += (sender, e) => PageClosing?.Invoke(sender, e);
			page.PageClosed += (sender, e) => PageClosed?.Invoke(sender, e);
			page.SlavePagesChanged += (sender, e) => SlavePagesChanged?.Invoke(sender, e);
			page.ChildPagesChanged += (sender, e) => ChildPagesChanged?.Invoke(sender, e);
		}

		#region IPage<TdiTabViewModelAdapter> implementation

		public TdiTabViewModelAdapter ViewModel => page.ViewModel;
		public string PageHash => page.PageHash;
		public IEnumerable<MasterToSlavePair> SlavePagesAll => page.SlavePagesAll;
		public IEnumerable<ParentToChildPair> ChildPagesAll => page.ChildPagesAll;
		public IEnumerable<IPage> SlavePages => page.SlavePages;
		public IEnumerable<IPage> ChildPages => page.ChildPages;
		public event EventHandler<PageClosingEventArgs> PageClosing;
		public event EventHandler PageClosed;
		public event EventHandler SlavePagesChanged;
		public event EventHandler ChildPagesChanged;

		#endregion IPage<TdiTabViewModelAdapter> implementation

		#region ITdiPage implementation

		public ITdiTab TdiTab => page.ViewModel.Tab;

		#endregion ITdiPage implementation

		#region IPage implementation

		ViewModelBase IPage.ViewModel => page.ViewModel;

		#endregion IPage implementation
	}
}
