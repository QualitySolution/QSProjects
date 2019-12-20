using System;
using Gtk;
using QS.Navigation.TabNavigation;
using System.Linq;
using QS.Project.Journal;

namespace QS.Navigation.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SliderTabPageView : Gtk.Bin
	{
		private readonly TabPageViewModelBase viewModel;
		private readonly IWidgetResolver widgetResolver;

		Widget dialogView;

		IPage journalPage;
		IPage dialogPage;

		private bool journalHided;
		public virtual bool JournalHided {
			get => journalHided;
			set {
				journalHided = value;
				UpdateButtonView();
			}
		}

		public SliderTabPageView(TabPageViewModelBase viewModel, IWidgetResolver widgetResolver)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			this.widgetResolver = widgetResolver ?? throw new ArgumentNullException(nameof(widgetResolver));

			journalPage = viewModel.Page;
			journalPage.ChildPagesChanged += (sender, e) => UpdateDialogPage();
			Widget journalView = widgetResolver.Resolve(viewModel.Page.ViewModel);
			boxJournalView.Add(journalView);

			buttonHide.Clicked += (sender, e) => JournalHided = !JournalHided;

			UpdateDialogPage();
		}

		private void UpdateDialogPage()
		{
			var newDialogPage = journalPage.ChildPages.FirstOrDefault();
			if(newDialogPage == null && dialogPage != null) {
				dialogPage = null;
				JournalHided = false;
				dialogView?.Destroy();
				dialogView = null;
				boxDialog.Visible = false;
			} 
			else if(newDialogPage != null && (dialogPage == null || dialogPage != newDialogPage)) {
				dialogPage = newDialogPage;
				JournalHided = (dialogPage.ViewModel as IJournalSlidedTab)?.SliderOption == SliderOption.UseSliderHided;
				dialogView?.Destroy();
				dialogView = widgetResolver.Resolve(dialogPage.ViewModel);
				boxDialogView.Add(dialogView);
				dialogView.Show();
				boxDialog.Visible = true;
			} else {
				boxDialog.Visible = dialogView != null;
			}
			UpdateButtonView();
		}

		private void UpdateButtonView()
		{
			buttonHide.Label = JournalHided ? ">" : "<";
			boxJournalView.Visible = !journalHided;
		}

		protected override void OnShown()
		{
			base.OnShown();
			UpdateDialogPage();
		}
	}
}
