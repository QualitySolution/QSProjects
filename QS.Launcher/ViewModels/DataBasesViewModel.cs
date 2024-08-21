using QS.Launcher.ViewModels.TypesViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace QS.Launcher.ViewModels;

public class DataBasesViewModel : PageViewModel
{
	public List<DatabaseViewModel> Databases { get; set; } = [];

	public DatabaseViewModel? SelectedDatabase { get; set; }

	public bool IsAdmin { get; } = true;

	public bool ShouldCloseLauncherAfterStart { get; set; } = false;
		
	public DataBasesViewModel(ICommand nextPageCommand, ICommand previousPageCommand) : base(nextPageCommand, previousPageCommand)
	{
		// for test
		Databases =
		[
			new() { Name = "Kukaracha.db", Size = "39.6 MB" },
			new() { Name = "Simon", Size = "501.0 MB" }
		];
	}

}
