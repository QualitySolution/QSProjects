using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QS.Navigation;
using QS.Project.Journal;

namespace QS.Journal.Views;

/// <summary>
/// Базовый класс для отображения журналов в Avalonia
/// </summary>
public partial class JournalView : UserControl
{
	private JournalViewModelBase? viewModel;
	protected IAvaloniaViewResolver? viewResolver;

	public JournalView()
	{
		InitializeComponent();
	}

	public JournalView(JournalViewModelBase viewModel, IAvaloniaViewResolver? viewResolver) : this()
	{
		this.viewResolver = viewResolver;
		ViewModel = viewModel;
		ConfigureJournal();
	}

	/// <summary>
	///  Свойство для установки контента таблицы (Вместо стандартного DataGrid).
	///  Это позволяет встраивать свои таблицы внутрь JournalView.
	/// </summary>
	public Control? TableContent
	{
		get => this.FindControl<ContentControl>("TablePlaceholder")?.Content as Control;
		set {
			var placeholder = this.FindControl<ContentControl>("TablePlaceholder");
			if (placeholder != null)
				placeholder.Content = value;
		}
	}


	public JournalViewModelBase? ViewModel
	{
		get => viewModel;
		set
		{
			viewModel = value;
			DataContext = value;
		}
	}

	// Поля buttonRefresh, checkShowFilter, filterContainer, searchContainer, 
	// dataGrid, labelFooter, actionsPanel генерируются автоматически Avalonia из XAML (x:Name)

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	private void ConfigureJournal()
	{
		if (ViewModel == null) return;

		// 1. Попытка загрузить кастомную таблицу (GridView) через резолвер
		// Для каждого журнала ДОЛЖНА существовать соответствующая GridView
		if (viewResolver == null)
		{
			throw new InvalidOperationException($"ViewResolver не установлен для журнала {ViewModel.GetType().Name}. Невозможно загрузить таблицу.");
		}
		
		var customTable = viewResolver.Resolve(ViewModel, "GridView");
		if (customTable == null)
		{
			throw new InvalidOperationException(
				$"Не найдена View с суффиксом 'GridView' для ViewModel типа '{ViewModel.GetType().FullName}'. " +
				$"Необходимо создать соответствующий UserControl (например, {{Name}}GridView) с таблицей данных.");
		}
		
		TableContent = customTable;

		// Подписываемся на события
		ViewModel.DataLoader.ItemsListUpdated += ViewModel_ItemsListUpdated;
		ViewModel.DataLoader.LoadingStateChanged += DataLoader_LoadingStateChanged;


		// Настраиваем режим выбора
		SetSelectionMode(ViewModel.TableSelectionMode);

		// Настраиваем фильтр
		ConfigureFilter();

		// Настраиваем поиск
		ConfigureSearch();


		// Загружаем данные
		Console.WriteLine("JournalView: Вызываем Refresh...");
		ViewModel.Refresh();
	}

	private void ConfigureFilter()
	{
		var filterContainer = this.FindControl<ContentControl>("filterContainer");
		
		if (ViewModel?.JournalFilter == null || filterContainer == null || viewResolver == null)
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
		var searchContainer = this.FindControl<ContentControl>("searchContainer");
		
		if (ViewModel?.Search == null || searchContainer == null || viewResolver == null || !ViewModel.SearchEnabled)
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
			// Обновление footer происходит через биндинг
		}
		else if (e.PropertyName == nameof(ViewModel.TableSelectionMode))
		{
			SetSelectionMode(ViewModel!.TableSelectionMode);
		}
	}

	private void ViewModel_ItemsListUpdated(object? sender, EventArgs e)
	{
		// Событие может вызываться из фонового потока, поэтому переключаемся на UI поток
		Avalonia.Threading.Dispatcher.UIThread.Post(() =>
		{
			var itemsCount = ViewModel?.Items?.Count ?? 0;
			Console.WriteLine($"JournalView: ItemsListUpdated, количество элементов: {itemsCount}");
			
			// Проверим DataGrid
			var grid = GetDataGrid();
			if (grid != null)
			{
				// Принудительно обновляем ItemsSource, так как автоматический биндинг может не подхватить изменение
				// если коллекция не Observable или NotifyPropertyChanged не сработал как надо
				if (ViewModel != null)
				{
					// Вариант 1: Сброс и установка заново
					// grid.ItemsSource = null;
					grid.ItemsSource = ViewModel.Items;
				}

				Console.WriteLine($"JournalView: DataGrid обновлен, ItemsSource count: {(grid.ItemsSource as System.Collections.IList)?.Count ?? -1}");
				Console.WriteLine($"JournalView: DataGrid.Columns.Count = {grid.Columns.Count}");
			}
			else
			{
				Console.WriteLine("JournalView: DataGrid не найден в ItemsListUpdated!");
			}
		});
	}

	private void DataLoader_LoadingStateChanged(object? sender, QS.Project.Journal.DataLoader.LoadingStateChangedEventArgs e)
	{
		// Событие может вызываться из фонового потока, поэтому переключаемся на UI поток
		Avalonia.Threading.Dispatcher.UIThread.Post(() =>
		{
			Console.WriteLine($"JournalView: LoadingStateChanged, состояние: {e.LoadingState}");
			// TODO: Добавить индикатор загрузки
		});
	}


	protected object[] GetSelectedItems()
	{
		var grid = GetDataGrid();
		if (grid?.SelectedItems == null) 
			return Array.Empty<object>();

		return grid.SelectedItems.Cast<object>().ToArray();
	}

	protected DataGrid? GetDataGrid()
	{
		var placeholder = this.FindControl<ContentControl>("TablePlaceholder");
		if (placeholder?.Content is Control content)
		{
			var grid = content.FindControl<DataGrid>("dataGrid");
			if (grid != null) return grid;
		}
		
		return this.FindControl<DataGrid>("dataGrid");
	}

	public void Dispose()
	{
		if (ViewModel != null)
		{
			ViewModel.DataLoader.ItemsListUpdated -= ViewModel_ItemsListUpdated;
			ViewModel.DataLoader.LoadingStateChanged -= DataLoader_LoadingStateChanged;
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
		}
	}
}

