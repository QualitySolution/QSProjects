using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ViewModelProperty)
            DataContext = change.NewValue;
    }

    private void OnSelectClicked(object? sender, RoutedEventArgs e)
        => ViewModel?.OpenSelectDialog();

    private void OnViewClicked(object? sender, RoutedEventArgs e)
        => ViewModel?.OpenViewEntity();

    private void OnCleanClicked(object? sender, RoutedEventArgs e)
        => ViewModel?.CleanEntity();
}

