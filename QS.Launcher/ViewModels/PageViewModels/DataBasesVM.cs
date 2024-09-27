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

	public bool ShouldCloseLauncherAfterStart { get; set; } = true;

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
		string fileName = launcherOptions.AppExecutablePath;

#if DEBUG
		DialogWindow.Success(resp.ConnectionString + "\nSession id: " + resp.Parameters.Find(p => p.Title == "SessionId").Value);
#endif

		Environment.SetEnvironmentVariable("QS_CONNECTION_STRING", resp.ConnectionString, EnvironmentVariableTarget.User);
		Environment.SetEnvironmentVariable("QS_LOGIN", provider.UserName, EnvironmentVariableTarget.User);
		foreach (ConnectionParameter par in resp.Parameters)
			Environment.SetEnvironmentVariable("QS_" + par.Title, par.Value as string, EnvironmentVariableTarget.User);

		Process.Start(new ProcessStartInfo {
			WorkingDirectory = "D:\\",
			FileName = fileName,
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
