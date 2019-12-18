using System;
using System.Collections.Generic;
using Gtk;
using QS.Navigation.TabNavigation;
using System.Collections.Specialized;
using System.Linq;
using QS.Project.Journal;

namespace QS.Navigation.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TabNavigatorView : Gtk.Bin
	{
		Notebook notebook;

		public TabNavigatorView()
		{
			this.Build();
		}

		private TabNavigatorViewModel tabNavigatorViewModel;
		public virtual TabNavigatorViewModel TabNavigatorViewModel {
			get => tabNavigatorViewModel;
			set {
				if(WidgetResolver == null) {
					throw new InvalidOperationException($"Должен быть установлен {nameof(WidgetResolver)}");
				}

				tabNavigatorViewModel = value;
				RefreshTabs();
				(tabNavigatorViewModel.PageViewModels as INotifyCollectionChanged).CollectionChanged += CollectionChanged;
			}
		}

		public IWidgetResolver WidgetResolver { get; set; }

		private class PageViewLink : IDisposable
		{
			public Widget TitleView { get; set; }
			public Widget ContentView { get; set; }
			public TabPageViewModelBase ViewModel { get; set; }

			public PageViewLink(Widget titleView, Widget contentView, TabPageViewModelBase viewModel)
			{
				TitleView = titleView;
				ContentView = contentView;
				ViewModel = viewModel;
			}

			#region IDisposable Support

			private bool disposedValue = false; // To detect redundant calls

			protected virtual void Dispose(bool disposing)
			{
				if(!disposedValue) {
					if(disposing) {
						// TODO: dispose managed state (managed objects).
					}

					// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
					// TODO: set large fields to null.

					ContentView?.Destroy();
					ContentView = null;
					TitleView?.Destroy();
					TitleView = null;

					disposedValue = true;
				}
			}

			// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
			~PageViewLink() {
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(false);
			}

			// This code added to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(true);
				// TODO: uncomment the following line if the finalizer is overridden above.
				GC.SuppressFinalize(this);
			}

			#endregion
		}

		private List<PageViewLink> tabViews = new List<PageViewLink>();

		private PageViewLink GetView(TabPageViewModelBase viewModel)
		{
			if(viewModel == null) {
				throw new ArgumentNullException(nameof(viewModel));
			}

			var viewLink = tabViews.FirstOrDefault(x => x.ViewModel == viewModel);
			if(viewLink != null) {
				return viewLink;
			}

			Widget titleView = new TabPageTitleView(viewModel);
			Widget contentView = null;
			if(viewModel.Page.ViewModel is JournalViewModelBase) {
				contentView = new SliderTabPageView(viewModel, WidgetResolver);
			} else {
				contentView = new TabPageView(viewModel, WidgetResolver);
			}

			var link = new PageViewLink(titleView, contentView, viewModel);
			tabViews.Add(link);

			viewModel.Page.PageClosed += (sender, e) => {
				if(tabViews.Contains(link)) {
					tabViews.Remove(link);
				}
				link.Dispose();
			};

			return link;
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action) {
				case NotifyCollectionChangedAction.Add:
					int addIndex = e.NewStartingIndex;
					foreach(var item in e.NewItems) {
						var viewLink = GetView(item as TabPageViewModelBase);
						if(viewLink == null) {
							continue;
						}
						notebook.AppendPage(viewLink.ContentView, viewLink.TitleView);
						//notebook.InsertPage(viewLink.ContentView, viewLink.TitleView, e.NewStartingIndex);
						notebook.ShowAll();
						viewLink.ContentView.Show();
						viewLink.TitleView.Show();
						notebook.ShowTabs = true;
						addIndex++;
					}
					break;
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Move:
					throw new NotSupportedException("Перемещение вкладок не поддерживается");
				case NotifyCollectionChangedAction.Remove:
					int removeIndex = e.OldStartingIndex;
					foreach(var item in e.OldItems) {
						notebook.RemovePage(removeIndex);
						removeIndex++;
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					RefreshTabs();
					break;
				default:
					break;
			}
		}

		private void RefreshTabs()
		{
			if(notebook != null) {
				notebook.Destroy();
			}
			notebook = new Notebook();
			hboxContent.Add(notebook);

			tabViews.Clear();

			foreach(TabPageViewModelBase pageViewModel in TabNavigatorViewModel.PageViewModels) {
				var viewLink = GetView(pageViewModel);
				notebook.AppendPage(viewLink.ContentView, viewLink.TitleView);
			}
			notebook.ShowTabs = true;
			notebook.ShowAll();
		}
	}
}
