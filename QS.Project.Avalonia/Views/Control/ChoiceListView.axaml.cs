using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using QS.Commands;
using QS.ViewModels.Control;

namespace QS.Views.Control;

/// <summary>
/// Список сущностей с флажками выбора, поиском с подсветкой и кнопками «Выбрать все» и «Снять все».
/// Аналог ChoiceListView из GTK. Работает поверх <see cref="IChoiceListViewModel"/>.
/// </summary>
public partial class ChoiceListView : UserControl
{
    public static readonly StyledProperty<IChoiceListViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<ChoiceListView, IChoiceListViewModel?>(nameof(ViewModel));

    public IChoiceListViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ICommand SelectAllCommand { get; }
    public ICommand UnSelectAllCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public ChoiceListView()
    {
        SelectAllCommand = new DelegateCommand(() => ViewModel?.SelectAll());
        UnSelectAllCommand = new DelegateCommand(() => ViewModel?.UnSelectAll());
        ClearSearchCommand = new DelegateCommand(() => SearchText.Text = string.Empty);
        InitializeComponent(true);

        // Подсветка зависит только от текста поиска и относится к этому виджету.
        SearchText.TextChanged += (_, _) => ViewModel?.HighlightLike(SearchText.Text ?? string.Empty);
    }
}
