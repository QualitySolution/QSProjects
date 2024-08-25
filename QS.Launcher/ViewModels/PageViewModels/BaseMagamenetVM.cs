using QS.Launcher.ViewModels.TypeViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;

class BaseMagamenetVM : CarouselPageVM {

	public List<DatabaseViewModel> Databases { get; set; } = [];

	public DatabaseViewModel? SelectedDatabase { get; set; }

	public BaseMagamenetVM(ICommand? nextPageCommand, ICommand? previousPageCommand, ICommand? changePageCommand)
		: base(nextPageCommand, previousPageCommand, changePageCommand)
	{
		Databases =
		[
			new() { Name = "Kukaracha.db", Size = "39.6 MB" },
			new() { Name = "Simon", Size = "501.0 MB" },
			new() { Name = "OnlyAdmins", Size = "1.0 MB" }
		];
	}
}
