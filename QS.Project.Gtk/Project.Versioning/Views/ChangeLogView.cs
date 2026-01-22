using QS.Project.Versioning.ViewModels;

namespace QS.Project.Versioning.Views {
	public partial class ChangeLogView : Gtk.Bin {
		
		private ChangeLogViewModel ViewModel;
		
		public ChangeLogView(ChangeLogViewModel viewModel) {
			this.Build();
			ViewModel = viewModel;
			
			ytextviewLog.WidthRequest = 700;
            ytextviewLog.HeightRequest = 500;
            ytextviewLog.WrapMode = Gtk.WrapMode.Word;
			            
			ytextviewLog.Binding.AddBinding(viewModel, vm => vm.TextLog, w => w.Buffer.Text).InitializeFromSource();
		}
	}
}
