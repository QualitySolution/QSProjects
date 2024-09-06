using Avalonia.Controls;
using System.Windows.Input;
using QS.Launcher.ViewModels;
using Avalonia.VisualTree;
using Avalonia.Input;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views.Pages; 
public partial class DataBasesView : UserControl
{
	public DataBasesView(DataBasesVM viewModel)
	{
		InitializeComponent();

		DataContext = viewModel;
	}

	public void Label_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if(!adminPanel.Classes.Remove("invisible"))
			adminPanel.Classes.Add("invisible");
	}
}
