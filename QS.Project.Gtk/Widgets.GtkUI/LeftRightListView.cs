using Gamma.Binding.Converters;
using Gamma.ColumnConfig;
using QS.ViewModels.Widgets;
using QS.Views.GtkUI;

namespace QS.Widgets.GtkUI {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LeftRightListView : WidgetViewBase<LeftRightListViewModel> {
		public LeftRightListView() {
			this.Build();
		}

		protected override void ConfigureWidget() {
			base.ConfigureWidget();

			ylabelLeft.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.LeftLabel, w => w.LabelProp)
				.InitializeFromSource();

			ylabelRight.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.RightLabel, w => w.LabelProp)
				.InitializeFromSource();

			ybuttonMoveRight.Clicked += (s, e) => ViewModel.MoveRightCommand.Execute();
			ViewModel.MoveRightCommand.CanExecuteChanged += (s, e) => ybuttonMoveRight.Sensitive = ViewModel.MoveRightCommand.CanExecute();
			ybuttonMoveRight.Sensitive = ViewModel.MoveRightCommand.CanExecute();

			ybuttonMoveLeft.Clicked += (s, e) => ViewModel.MoveLeftCommand.Execute();
			ViewModel.MoveLeftCommand.CanExecuteChanged += (s, e) => ybuttonMoveLeft.Sensitive = ViewModel.MoveLeftCommand.CanExecute();
			ybuttonMoveLeft.Sensitive = ViewModel.MoveLeftCommand.CanExecute();

			ybuttonMoveUp.Clicked += (s, e) => ViewModel.MoveUpCommand.Execute();
			ViewModel.MoveUpCommand.CanExecuteChanged += (s, e) => ybuttonMoveUp.Sensitive = ViewModel.MoveUpCommand.CanExecute();
			ybuttonMoveUp.Sensitive = ViewModel.MoveUpCommand.CanExecute();

			ybuttonMoveDown.Clicked += (s, e) => ViewModel.MoveDownCommand.Execute();
			ViewModel.MoveDownCommand.CanExecuteChanged += (s, e) => ybuttonMoveDown.Sensitive = ViewModel.MoveDownCommand.CanExecute();
			ybuttonMoveDown.Sensitive = ViewModel.MoveDownCommand.CanExecute();

			ytreeviewLeft.HeadersVisible = false;
			ytreeviewLeft.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewLeft.ColumnsConfig = FluentColumnsConfig<LeftRightListItemViewModel>.Create()
				.AddColumn("").HeaderAlignment(0.5f)
				.AddTextRenderer(x => x.Name).XAlign(0f)
				.Finish();
			ytreeviewLeft.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.LeftItems, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedLeftItems, w => w.SelectedRows, new ArrayToEnumerableConverter<LeftRightListItemViewModel>())
				.InitializeFromSource();

			ytreeviewRight.HeadersVisible = false;
			ytreeviewRight.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewRight.ColumnsConfig = FluentColumnsConfig<LeftRightListItemViewModel>.Create()
				.AddColumn("").HeaderAlignment(0.5f)
				.AddTextRenderer(x => x.Name).XAlign(0f)
				.Finish();
			ytreeviewRight.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.RightItems, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedRightItems, w => w.SelectedRows, new ArrayToEnumerableConverter<LeftRightListItemViewModel>())
				.InitializeFromSource();
		}
	}
}
