using DynamicData.Kernel;
using QS.DbManagement;
using QS.Launcher.ViewModels.Commands;
using QS.Launcher.ViewModels.TypeViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;

public class DataBasesVM : CarouselPageVM {

	private IDbProvider provider;
	public IDbProvider Provider {
		get => provider;
		set {
			this.RaiseAndSetIfChanged(ref provider, value);
			Databases = provider.GetUserDatabases().AsList();
			this.RaisePropertyChanged(nameof(Databases));
		}
	}

	public List<DbInfo> Databases { get; set; } = [];

	public DatabaseViewModel? SelectedDatabase { get; set; }

	public bool IsAdmin { get; set; } = false;

	public bool ShouldCloseLauncherAfterStart { get; set; } = false;

	public DataBasesVM(NextPageCommand? nextPageCommand, PreviousPageCommand? previousPageCommand,
		ChangePageCommand? changePageCommand) : base(nextPageCommand, previousPageCommand, changePageCommand)
	{
		
	}

}
