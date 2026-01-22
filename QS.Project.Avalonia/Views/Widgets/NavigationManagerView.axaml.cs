using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using QS.Navigation;

namespace QS.Project;

/// <summary>
/// Avalonia-based NavigationManager with tabs and navi menu.
/// This control requires FluentAvaloniaTheme in your App.axaml.
/// </summary>
public partial class NavigationManagerView : UserControl {
	public NavigationManagerView() {
		InitializeComponent();
	}

	public AvaloniaNavigationManager? NavigationManager => DataContext as AvaloniaNavigationManager;

	private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) {
		var page = args.Item as IAvaloniaPage;
		if(page != null && NavigationManager != null) {
			NavigationManager.AskClosePage(page, CloseSource.ClosePage);
		}
	}
}
