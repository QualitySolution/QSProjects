using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using QS.Dialog;
using QS.Journal.Columns;
using QS.Navigation;
using QS.Project.Journal;

namespace QS.Journal.Views;

/// <summary>
/// Базовый класс для отображения журналов в Avalonia
/// </summary>
public partial class JournalView : UserControl, IDisposable
{
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	private IJournalViewModel? viewModel;
	private readonly IGuiDispatcher? guiDispatcher;
	private readonly JournalColumnsRegistry? columnsRegistry;
	protected IAvaloniaViewResolver? viewResolver;

	public JournalView()
	{
		InitializeComponent();
	}

	public JournalView(IJournalViewModel viewModel, IAvaloniaViewResolver? viewResolver, IGuiDispatcher guiDispatcher,
		JournalColumnsRegistry columnsRegistry) : this()
	{
		this.viewResolver = viewResolver;
		this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
		this.columnsRegistry = columnsRegistry ?? throw new ArgumentNullException(nameof(columnsRegistry));
		ViewModel = viewModel;
		ConfigureJournal();
	}

	/// <summary>
	///  Свойство для установки контента таблицы (Вместо стандартного DataGrid).
	///  Это позволяет встраивать свои таблицы внутрь JournalView.
	/// </summary>
	public Control? TableContent
	{
		get => TablePlaceholder.Content as Control;
		set => TablePlaceholder.Content = value;
	}


	public IJournalViewModel? ViewModel
	{
		get => viewModel;
		set
		{
			viewModel = value;
			DataContext = value;
		}
	}

	private void ConfigureJournal()
	{
		if (ViewModel == null) return;

		TableContent = MakeTable();

		// Подписываемся на события
		ViewModel.DataLoader.ItemsListUpdated += ViewModel_ItemsListUpdated;
		ViewModel.DataLoader.LoadingStateChanged += DataLoader_LoadingStateChanged;
		ViewModel.PropertyChanged += OnViewModelPropertyChanged;

		UpdateFooter();

		// Настраиваем режим выбора
		SetSelectionMode(ViewModel.TableSelectionMode);

		// Подписываемся на события таблицы для передачи в ActionsViewModel
		ConfigureDataGridEvents();

		// Настраиваем фильтр
		ConfigureFilter();

		// Настраиваем поиск
		ConfigureSearch();


		// Загружаем данные
		ViewModel.Refresh();
	}

	/// <summary>
	/// Колонки журнала описаны кодом — собираем таблицу сами. Если нет, ищем вью таблицы
	/// по суффиксу GridView: так устроены журналы, которые описывают колонки разметкой.
	/// </summary>
	private Control MakeTable()
	{
		var columns = columnsRegistry!.Resolve(ViewModel!);
		if (columns != null)
			return columns.MakeTable();

		return viewResolver?.Resolve(ViewModel!, "GridView")
			?? throw new InvalidOperationException(
				$"Не найдены колонки для журнала '{ViewModel!.GetType().FullName}'. " +
				$"Опишите их в JournalColumnsRegistry либо создайте UserControl {{Name}}GridView с таблицей данных.");
	}

	private void ConfigureFilter()
	{
		if (ViewModel?.JournalFilter == null || viewResolver == null)
		{
			return;
		}

		// Проверяем, что фильтр является ViewModelBase
		if (ViewModel.JournalFilter is QS.ViewModels.ViewModelBase filterViewModel)
		{
			var filterView = viewResolver.Resolve(filterViewModel);
			if (filterView != null)
			{
				filterContainer.Content = filterView;
			}
		}
	}

	private void ConfigureSearch()
	{
		if (ViewModel?.Search == null || viewResolver == null || !ViewModel.SearchEnabled)
			return;

		// Проверяем, что поиск является ViewModelBase
		if (ViewModel.Search is QS.ViewModels.ViewModelBase searchViewModel)
		{
			var searchView = viewResolver.Resolve(searchViewModel);
			if (searchView != null)
			{
				searchContainer.Content = searchView;
			}
		}
	}

	private void ConfigureDataGridEvents()
	{
		var grid = GetDataGrid();
		if (grid == null) return;

		// Подписываемся на изменение выбора
		grid.SelectionChanged += DataGrid_SelectionChanged;
		
		// Подписываемся на двойной клик
		grid.DoubleTapped += DataGrid_DoubleTapped;
		
		// Подписываемся на нажатие клавиш
		grid.KeyDown += DataGrid_KeyDown;
	}

	private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (ViewModel?.ActionsViewModel == null) return;

		var selectedItems = GetSelectedItems();
		ViewModel.ActionsViewModel.OnSelectionChanged(selectedItems);
	}

	private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
	{
		if (ViewModel?.ActionsViewModel == null) return;

		var selectedItems = GetSelectedItems();
		if (selectedItems.Length > 0)
		{
			// Пытаемся получить информацию о колонке (опционально)
			ViewModel.ActionsViewModel.OnCellDoubleClick(selectedItems[0], null, null);
		}
	}

	private void DataGrid_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
	{
		if (ViewModel?.ActionsViewModel == null) return;

		// Преобразуем клавишу в строку
		var key = e.Key.ToString();
		ViewModel.ActionsViewModel.OnKeyPressed(key);
	}

	private void SetSelectionMode(JournalSelectionMode mode)
	{
		var grid = GetDataGrid();
		if (grid == null) return;

		grid.SelectionMode = mode switch
		{
			JournalSelectionMode.None => DataGridSelectionMode.Single,
			JournalSelectionMode.Single => DataGridSelectionMode.Single,
			JournalSelectionMode.Multiple => DataGridSelectionMode.Extended,
			_ => DataGridSelectionMode.Single
		};
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ViewModel.FooterInfo))
		{
			guiDispatcher!.RunInGuiTread(UpdateFooter);
		}
		else if (e.PropertyName == nameof(ViewModel.TableSelectionMode))
		{
			SetSelectionMode(ViewModel!.TableSelectionMode);
		}
	}

	private void ViewModel_ItemsListUpdated(object? sender, EventArgs e)
	{
		// Событие может вызываться из фонового потока, поэтому переключаемся на UI поток
		guiDispatcher!.RunInGuiTread(() =>
		{
			var grid = GetDataGrid();
			if (grid == null || ViewModel == null)
			{
				logger.Warn("Таблица журнала {0} не найдена, обновлять нечего.", ViewModel?.GetType().Name);
				return;
			}

			// Принудительно обновляем ItemsSource
			grid.ItemsSource = ViewModel.Items;
			UpdateFooter();
		});
	}

	// FooterInfo считается по загруженным строкам и об изменении не уведомляет
	private void UpdateFooter() => labelFooter.Text = ViewModel!.FooterInfo;

	private void DataLoader_LoadingStateChanged(object? sender, QS.Project.Journal.DataLoader.LoadingStateChangedEventArgs e)
	{
		// Событие может вызываться из фонового потока
		guiDispatcher!.RunInGuiTread(() =>
		{
			loadingIndicator.IsVisible = e.LoadingState == QS.Project.Journal.DataLoader.LoadingState.InProgress;
		});
	}

	protected object[] GetSelectedItems()
	{
		var grid = GetDataGrid();
		if (grid?.SelectedItems == null) 
			return Array.Empty<object>();

		return grid.SelectedItems.Cast<object>().ToArray();
	}

	protected DataGrid? GetDataGrid() =>
		// Таблицу мы собрали сами по описанию колонок — она и лежит в подставке
		// если журнал описывает колонки разметкой, таблица внутри чужой вью, ищем её по имени
		TablePlaceholder.Content as DataGrid
			?? (TablePlaceholder.Content as Control)?.FindControl<DataGrid>("dataGrid");

	public virtual void Dispose()
	{
		if (ViewModel != null)
		{
			ViewModel.DataLoader.ItemsListUpdated -= ViewModel_ItemsListUpdated;
			ViewModel.DataLoader.LoadingStateChanged -= DataLoader_LoadingStateChanged;
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
		}

		var grid = GetDataGrid();
		if (grid != null)
		{
			grid.SelectionChanged -= DataGrid_SelectionChanged;
			grid.DoubleTapped -= DataGrid_DoubleTapped;
			grid.KeyDown -= DataGrid_KeyDown;
		}
	}
}
