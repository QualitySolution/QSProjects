using Avalonia.Controls.ApplicationLifetimes;
using DynamicData.Kernel;
using QS.DbManagement;
using QS.Launcher.ViewModels.Commands;
using QS.Project.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
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

	private DbInfo? selectedDatabase;
	public DbInfo? SelectedDatabase {
		get => selectedDatabase;
		set => this.RaiseAndSetIfChanged(ref selectedDatabase, value);
	}

	public bool IsAdmin { get; set; } = false;

	public bool ShouldCloseLauncherAfterStart { get; set; } = false;

	public ICommand ConnectCommand { get; }

	private LauncherOptions launcherOptions;

	public DataBasesVM(NextPageCommand? nextPageCommand, PreviousPageCommand? previousPageCommand,
		ChangePageCommand? changePageCommand, LauncherOptions options)
		: base(nextPageCommand, previousPageCommand, changePageCommand)
	{
		launcherOptions = options;

		IObservable<bool>? canExecute = this
			.WhenAnyValue(x => x.SelectedDatabase)
			.Select(x => x is not null);
		ConnectCommand = ReactiveCommand.Create(Connect, canExecute);
	}

	public void Connect() {

		var resp = provider.LoginToDatabase(SelectedDatabase);

		if(!resp.Success) {
			DialogWindow.Error(resp.ErrorMessage);
			return;
		}
		string fn = launcherOptions.AppExecutablePath;
		DialogWindow.Success(resp.ConnectionString);
		Process.Start(new ProcessStartInfo {
			WorkingDirectory = "D:\\",
			FileName = fn,
			UseShellExecute = true,
			CreateNoWindow = true,
			Arguments = resp.ConnectionString
		});

		if (ShouldCloseLauncherAfterStart) {
			var window = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
			window?.Close();
		}
	}
}
