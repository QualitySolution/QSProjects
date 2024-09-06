using QS.Launcher.ViewModels.Commands;
using System;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;
public class UserManagementVM : CarouselPageVM {
	public UserManagementVM(NextPageCommand? nextPageCommand, PreviousPageCommand? previousPageCommand, ChangePageCommand? changePageCommand)
		: base(nextPageCommand, previousPageCommand, changePageCommand)
	{

	}
}
