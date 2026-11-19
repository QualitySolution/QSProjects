using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace QS.Project.Avalonia.Themes;

public static class NavigationViewFlyout {
	public static readonly AttachedProperty<PlacementMode?> PlacementProperty =
		AvaloniaProperty.RegisterAttached<Grid, PlacementMode?>("Placement", typeof(NavigationViewFlyout));

	static NavigationViewFlyout() {
		PlacementProperty.Changed.AddClassHandler<Grid>((grid, _) => {
			// FluentAvalonia может оставить стандартное положение Bottom при первой загрузке шаблона.
			if(GetPlacement(grid) is { } placement && FlyoutBase.GetAttachedFlyout(grid) is PopupFlyoutBase flyout) {
				flyout.Placement = placement;
			}
		});
	}

	public static PlacementMode? GetPlacement(Grid grid) => grid.GetValue(PlacementProperty);

	public static void SetPlacement(Grid grid, PlacementMode? value) => grid.SetValue(PlacementProperty, value);
}
