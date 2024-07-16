using System.Windows.Input;

namespace QS.Launcher.ViewModels;

public class DataBasesViewModel(ICommand? nextPageCommand, ICommand? backPageCommand)
	: PageViewModel(nextPageCommand, backPageCommand)
{

}
