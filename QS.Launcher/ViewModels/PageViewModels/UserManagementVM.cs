using System;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;
public class UserManagementVM : CarouselPageVM {
	public UserManagementVM(ICommand nextPageCommand, ICommand previousPageCommand, ICommand changePageCommand)
		: base(nextPageCommand, previousPageCommand, changePageCommand)
	{

	}
}
