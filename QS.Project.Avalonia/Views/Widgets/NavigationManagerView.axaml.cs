using Avalonia.Controls;
using Avalonia.Interactivity;
using QS.Navigation;
using QS.Project.Avalonia.Interactive;

namespace QS.Project;

/// <summary>
/// Avalonia-based NavigationManager with tabs and navi menu.
/// This control requires FluentAvaloniaTheme in your App.axaml.
/// </summary>
public partial class NavigationManagerView : UserControl {
	public NavigationManagerView() {
		InitializeComponent();

		DataContext = new AvaloniaNavigationManager(new AvaloniaInteractiveMessage(), null, null);
	}

	public AvaloniaNavigationManager NavigationManager => DataContext as AvaloniaNavigationManager;

	private void OnCloseTabClick(object sender, RoutedEventArgs e) {
		var dc = (sender as Control).DataContext as IAvaloniaPage;
		NavigationManager.AskClosePage(dc, CloseSource.ClosePage);
	}
}
