using System;
using Gtk;
using QS.Navigation.TabNavigation;

namespace QS.Navigation.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TabPageTitleView : Gtk.Bin
	{
		private readonly TabPageViewModelBase viewModel;

		public TabPageTitleView(TabPageViewModelBase viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

			HBox box = new HBox();
			Label nameLable = new Label(viewModel.Title);
			box.Add(nameLable);
			Image closeImage = new Image(Stock.Close, IconSize.Menu);
			Button closeButton = new Button(closeImage);
			closeButton.Relief = ReliefStyle.None;

			closeButton.Clicked += (sender, e) => viewModel.CloseCommand.Execute();
			viewModel.CloseCommand.CanExecuteChanged += (sender, e) => closeButton.Sensitive = viewModel.CloseCommand.CanExecute();

			closeButton.ModifierStyle.Xthickness = 0;
			closeButton.ModifierStyle.Ythickness = 0;
			box.Add(closeButton);
			Add(box);
			ShowAll();
		}
	}
}
