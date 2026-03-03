using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QS.ViewModels.Control.EEVM;

namespace QS.Views.Widgets;

public partial class EntityEntryView : UserControl
{
    public static readonly StyledProperty<IEntityEntryViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<EntityEntryView, IEntityEntryViewModel?>(nameof(ViewModel));

    public IEntityEntryViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public EntityEntryView()
    {
        InitializeComponent(true);
        UpdateNoModelState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ViewModelProperty)
            UpdateNoModelState();
    }

    private void UpdateNoModelState()
    {
        bool has = ViewModel != null;

        if (EntryText    != null) EntryText.Watermark    = has ? "(не выбрано)" : "(нет модели)";
        if (ButtonSelect != null) ButtonSelect.IsEnabled = has && ViewModel!.SensitiveSelectButton;
        if (ButtonView   != null) ButtonView.IsEnabled   = has && ViewModel!.SensitiveViewButton;
        if (ButtonClean  != null) ButtonClean.IsVisible  = has && ViewModel!.SensitiveCleanButton;
    }

    private void OnSelectClicked(object? sender, RoutedEventArgs e)
        => ViewModel?.OpenSelectDialog();

    private void OnViewClicked(object? sender, RoutedEventArgs e)
        => ViewModel?.OpenViewEntity();

    private void OnCleanClicked(object? sender, RoutedEventArgs e)
        => ViewModel?.CleanEntity();
}
