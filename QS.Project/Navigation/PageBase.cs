using System;
using System.Collections.Generic;
using System.Linq;
using QS.ViewModels;

namespace QS.Navigation
{
	public abstract class PageBase: IPage, IPageInternal
	{
		protected PageBase()
		{
		}

		public event EventHandler PageClosed;
		public event EventHandler SlavePagesChanged;
		public event EventHandler ChildPagesChanged;

		public string PageHash { get; protected set; }

		public IEnumerable<MasterToSlavePair> SlavePagesAll {
			get {
				foreach (var item in slavePages)
					yield return new MasterToSlavePair { MasterPage = this, SlavePage = item };

				foreach (var pair in ChildPagesAll) {
					foreach (var item in pair.ChildPage.SlavePagesAll) {
						yield return item;
					}
				}
			}
		}

		public IEnumerable<ParentToChildPair> ChildPagesAll {
			get {
				foreach (var child in childPages) {
					yield return new ParentToChildPair { ParentPage = this, ChildPage = child };
					foreach (var item in child.ChildPagesAll) {
						yield return item;
					}
				}
			}
		}

		public IEnumerable<IPage> SlavePages => slavePages;

		public IEnumerable<IPage> ChildPages => childPages;

		ViewModelBase IPage.ViewModel => null;

		#region Приватное

		private readonly List<IPage> slavePages = new List<IPage>();

		private readonly List<IPage> childPages = new List<IPage>();

		#endregion

		#region IPageInternal

		void IPageInternal.OnClosed()
		{
			PageClosed?.Invoke(this, EventArgs.Empty);
		}

		void IPageInternal.AddSlavePage(IPage page)
		{
			slavePages.Add(page);
			SlavePagesChanged?.Invoke(this, EventArgs.Empty);
		}

		bool IPageInternal.RemoveSlavePage(IPage page)
		{
			var result = slavePages.Remove(page);
			if(result) {
				SlavePagesChanged?.Invoke(this, EventArgs.Empty);
				return result;
			}

			var pair = SlavePagesAll.FirstOrDefault(x => x.SlavePage == page);
			if (pair == null)
				return false;
			result = (pair.MasterPage as IPageInternal).RemoveSlavePage(page);
			if(result) {
				SlavePagesChanged?.Invoke(this, EventArgs.Empty);
			}
			return result;
		}

		void IPageInternal.AddChildPage(IPage page)
		{
			childPages.Add(page);
			ChildPagesChanged?.Invoke(this, EventArgs.Empty);
		}

		bool IPageInternal.RemoveChildPage(IPage page)
		{
			var result = childPages.Remove(page);
			if(result) {
				ChildPagesChanged?.Invoke(this, EventArgs.Empty);
				return result;
			}

			var pair = ChildPagesAll.FirstOrDefault(x => x.ChildPage == page);
			if (pair == null)
				return false;
			result = (pair.ParentPage as IPageInternal).RemoveChildPage(page);
			if(result) {
				ChildPagesChanged?.Invoke(this, EventArgs.Empty);
			}
			return result;
		}

		#endregion
	}
}
