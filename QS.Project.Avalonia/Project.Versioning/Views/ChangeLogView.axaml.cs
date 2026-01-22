using Avalonia.Controls;
using QS.Project.Versioning.ViewModels;

namespace QS.Project.Versioning.Views
{
	public partial class ChangeLogView : UserControl
	{
		private ChangeLogViewModel ViewModel => DataContext as ChangeLogViewModel;

		public ChangeLogView()
		{
			InitializeComponent();
		}

		public ChangeLogView(ChangeLogViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}
}

