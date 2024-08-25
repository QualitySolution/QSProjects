using QS.Launcher.ViewModels.TypeViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;

public class DataBasesVM : CarouselPageVM {
	public List<DatabaseViewModel> Databases { get; set; } = [];

	public DatabaseViewModel? SelectedDatabase { get; set; }

	public bool IsAdmin { get; } = true;

	public bool ShouldCloseLauncherAfterStart { get; set; } = false;

	public DataBasesVM(ICommand? nextPageCommand, ICommand? previousPageCommand, ICommand? changePageCommand)
		: base(nextPageCommand, previousPageCommand, changePageCommand)
	{
		// for test
		Databases =
		[
			new() { Name = "Kukaracha.db", Size = "39.6 MB" },
			new() { Name = "Simon", Size = "501.0 MB" }
		];
	}

}
