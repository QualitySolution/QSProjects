using Avalonia.Controls;
using System.Windows.Input;
using QS.Launcher.ViewModels;
using Avalonia.VisualTree;
using Avalonia.Input;

namespace QS.Launcher.Views; 
public partial class DataBasesView : UserControl
{
	public DataBasesView(ICommand? nextPageCommand, ICommand? backPageCommand)
	{
		InitializeComponent();

		DataContext = new DataBasesViewModel(nextPageCommand, backPageCommand);
	}

	public void Label_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if(!adminPanel.Classes.Remove("invisible"))
			adminPanel.Classes.Add("invisible");
	}
}
