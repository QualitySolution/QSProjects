using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using QS.Navigation;
using QS.Updater;
using QS.Updater.App;
using QS.Updater.App.ViewModels;
using QS.Updates;

namespace QS.Launcher.Updates;

public sealed class LauncherUpdateService : ILauncherUpdateService {
	private readonly ApplicationUpdateChecker checker;
	private readonly ISkipVersionState skip;
	private readonly IUpdateChannelService channels;
	private readonly AvaloniaNavigationManager navigation;
	private ReleaseInfo[] available = Array.Empty<ReleaseInfo>();
	private bool dialogOpen;
	public event Action? UpdatesDisabled;

	public LauncherUpdateService(ApplicationUpdateChecker checker, ISkipVersionState skip,
		IUpdateChannelService channels, AvaloniaNavigationManager navigation) {
		this.checker = checker;
		this.skip = skip;
		this.channels = channels;
		this.navigation = navigation;
	}

	public async Task<LauncherUpdateResult?> CheckAsync(CancellationToken cancellationToken) {
		var response = await checker.CheckAsync(cancellationToken);
		available = response.Releases.ToArray();
		if(available.Length == 0) return null;
		var version = Version.Parse(available[0].Version);
		return new LauncherUpdateResult { Version = available[0].Version, IsSkipped = skip.IsSkippedVersion(version) };
	}

	public void ShowUpdate() {
		if(dialogOpen || available.Length == 0) return;
		dialogOpen = true;
		try {
			var page = navigation.OpenViewModelAsWindow<NewVersionViewModel, ReleaseInfo[]>(
				null!, available, configureWindow: window => {
					window.SizeToContent = SizeToContent.Manual;
					window.MinWidth = Math.Min(540, window.MaxWidth);
					window.MinHeight = Math.Min(450, window.MaxHeight);
					window.Width = Math.Min(800, window.MaxWidth);
					window.Height = Math.Min(560, window.MaxHeight);
				});
			page.PageClosed += (_, args) => {
				dialogOpen = false;
				if(channels.CurrentChannel == UpdateChannel.Off) {
					available = Array.Empty<ReleaseInfo>();
					UpdatesDisabled?.Invoke();
				}
				if(args.CloseSource == CloseSource.AppQuit && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
					desktop.Shutdown();
			};
		}
		catch { dialogOpen = false; throw; }
	}

}
