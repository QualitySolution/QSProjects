using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using FluentAvalonia.Styling;
using QS.Launcher.Views;
using System;
using System.Linq;

namespace QS.Launcher;

public partial class LauncherApp() : Application
{
	private static readonly Uri QsProjectThemeSource =
		new("avares://QS.Project.Avalonia/Themes/QSProjectTheme.axaml");

	public Func<MainWindow> MainWindowGetter { get; set; }
	public Uri? PaletteSource { get; set; }

	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);

		if(PaletteSource is not null)
			ApplyProductTheme(PaletteSource);
	}

	private void ApplyProductTheme(Uri paletteSource) {
		Resources.MergedDictionaries.Add(new ResourceInclude(paletteSource) {
			Source = paletteSource
		});

		Styles.Add(new StyleInclude(QsProjectThemeSource) {
			Source = QsProjectThemeSource
		});

		if(Resources.TryGetResource("QsColorSystemAccent", ActualThemeVariant, out var accentResource)
			&& accentResource is Color accentColor) {
			var fluentTheme = Styles.OfType<FluentAvaloniaTheme>().First();
			fluentTheme.PreferUserAccentColor = false;
			fluentTheme.CustomAccentColor = accentColor;
		}
	}

	public override void OnFrameworkInitializationCompleted() {
		if (MainWindowGetter is null)
			throw new ArgumentNullException(nameof(MainWindowGetter));

		if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = MainWindowGetter();
		
		base.OnFrameworkInitializationCompleted();
	}
}
