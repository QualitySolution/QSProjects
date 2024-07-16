using Avalonia.Controls;
using System.Windows.Input;
using QS.Launcher.ViewModels;
using Avalonia.VisualTree;

namespace QS.Launcher.Views; 
public partial class DataBasesView : UserControl
{
	public DataBasesView(ICommand? nextPageCommand, ICommand? backPageCommand)
	{
		InitializeComponent();

		DataContext = new DataBasesViewModel(nextPageCommand, backPageCommand);
	}
}
