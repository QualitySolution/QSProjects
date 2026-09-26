using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Threading;
using QS.ViewModels.Control.EEVM;
using System;
using System.ComponentModel;
using System.Linq;

namespace QS.Views.Widgets;

public partial class EntityEntryView : UserControl
{
    /// <summary>Сколько строк запрашиваем у автодополнения</summary>
    private const int autocompleteListSize = 20;

    private IEntityEntryViewModel? subscribedViewModel;

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

        EntryText.Populating += OnPopulating;
        EntryText.ItemSelector = (_, item) => ViewModel?.GetAutocompleteTitle(item) ?? String.Empty;
        EntryText.ItemTemplate = new FuncDataTemplate<object>(
            (item, _) => new TextBlock { Text = ViewModel?.GetAutocompleteTitle(item) ?? String.Empty });
        EntryText.SelectionChanged += OnAutocompleteSelectionChanged;
        EntryText.LostFocus += OnEntryLostFocus;

        UpdateNoModelState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ViewModelProperty)
            SubscribeViewModel();
    }

    private void SubscribeViewModel()
    {
        if (subscribedViewModel != null)
        {
            subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            subscribedViewModel.AutoCompleteListUpdated -= OnAutocompleteListUpdated;
        }

        subscribedViewModel = ViewModel;

        if (subscribedViewModel != null)
        {
            subscribedViewModel.AutocompleteListSize = autocompleteListSize;
            subscribedViewModel.PropertyChanged += OnViewModelPropertyChanged;
            subscribedViewModel.AutoCompleteListUpdated += OnAutocompleteListUpdated;
        }

        UpdateNoModelState();
        SetEntryText();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IEntityEntryViewModel.EntityTitle))
            SetEntryText();
    }

    private void SetEntryText() => EntryText.Text = ViewModel?.EntityTitle ?? String.Empty;

    private void UpdateNoModelState()
    {
        bool has = ViewModel != null;

        if (EntryText    != null) EntryText.PlaceholderText = has ? "(не выбрано)" : "(нет модели)";
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

    #region Автодополнение

    private void OnPopulating(object? sender, PopulatingEventArgs e)
    {
        e.Cancel = true;
        var viewModel = ViewModel;
        if (viewModel == null || !viewModel.SensitiveAutoCompleteEntry || !EntryText.IsKeyboardFocusWithin)
            return;

        viewModel.AutocompleteTextEdited(e.Parameter ?? String.Empty);
    }

    private void OnAutocompleteListUpdated(object? sender, AutocompleteUpdatedEventArgs e)
    {
        var items = e.List.Cast<object>().ToArray();
        Dispatcher.UIThread.Post(() =>
        {
            EntryText.ItemsSource = items;
            EntryText.PopulateComplete();
        });
    }

    private void OnAutocompleteSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is object node)
            ViewModel?.AutocompleteSelectNode(node);
    }

    private void OnEntryLostFocus(object? sender, RoutedEventArgs e)
    {
        var viewModel = ViewModel;
        if (viewModel == null || EntryText.Text == viewModel.EntityTitle)
            return;

        if (String.IsNullOrWhiteSpace(EntryText.Text))
            viewModel.CleanEntity();
        else
            SetEntryText();
    }

    #endregion
}
