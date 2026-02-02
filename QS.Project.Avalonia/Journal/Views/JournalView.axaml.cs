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

	/// <summary>
	/// Событие для настройки колонок после полной инициализации контролов
	/// </summary>
	public event EventHandler? ColumnsConfigurationRequired;

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
		// 1. Загружаем XAML текущего экземпляра (это может быть наследник ClientJournalView)
		// Это применит свойства, определенные в XAML наследника (например GridTemplate)
		ViewModel.DataLoader.ItemsListUpdated += ViewModel_ItemsListUpdated;
		ViewModel.DataLoader.LoadingStateChanged += DataLoader_LoadingStateChanged;

		// Кэшируем контролы. Ищем их в this (визуальное дерево уже должно быть привязано к this)
		ViewModel.UpdateJournalActions += UpdateButtonActions;

		// Получаем контролы
		var buttonRefresh = this.FindControl<Button>("buttonRefresh");
		var checkShowFilter = this.FindControl<CheckBox>("checkShowFilter");

		// Настраиваем кнопки
		if (buttonRefresh != null)
			buttonRefresh.Click += (_, _) => ViewModel.Refresh();

		if (checkShowFilter != null)
		{
			checkShowFilter.IsChecked = ViewModel.IsFilterShow;
			checkShowFilter.Click += (_, _) => ViewModel.IsFilterShow = checkShowFilter.IsChecked ?? false;
		}

		// Настраиваем режим выбора
		SetSelectionMode(ViewModel.TableSelectionMode);

		// Настраиваем фильтр
		ConfigureFilter();

		// Настраиваем поиск
		ConfigureSearch();

		// Настраиваем действия
		UpdateButtonActions();

		// Загружаем данные
		Console.WriteLine("JournalView: Вызываем Refresh...");
		ViewModel.Refresh();
		
		// Настраиваем колонки ПОСЛЕ всех инициализаций через Dispatcher
		Console.WriteLine("JournalView: Планируем вызов ConfigureColumns через Dispatcher...");
		Avalonia.Threading.Dispatcher.UIThread.Post(() =>
		{
			Console.WriteLine("JournalView: Вызываем ConfigureColumns из Dispatcher...");
			ColumnsConfigurationRequired?.Invoke(this, EventArgs.Empty);
		}, Avalonia.Threading.DispatcherPriority.Loaded);
	}

	private void ConfigureFilter()
	{
		var filterContainer = this.FindControl<ContentControl>("filterContainer");
		var checkShowFilter = this.FindControl<CheckBox>("checkShowFilter");
		
		if (ViewModel?.JournalFilter == null || filterContainer == null || viewResolver == null)
		{
			if (checkShowFilter != null)
				checkShowFilter.IsVisible = false;
			return;
		}

		// Проверяем, что фильтр является ViewModelBase
		if (ViewModel.JournalFilter is QS.ViewModels.ViewModelBase filterViewModel)
		{
			var filterView = viewResolver.Resolve(filterViewModel);
			if (filterView != null)
			{
				filterContainer.Content = filterView;
				if (checkShowFilter != null)
					checkShowFilter.IsVisible = true;
			}
		}
		else
		{
			if (checkShowFilter != null)
				checkShowFilter.IsVisible = false;
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
		else if (e.PropertyName == nameof(ViewModel.IsFilterShow))
		{
			var checkShowFilter = this.FindControl<CheckBox>("checkShowFilter");
			if (checkShowFilter != null)
				checkShowFilter.IsChecked = ViewModel!.IsFilterShow;
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

	private void UpdateButtonActions()
	{
		var actionsPanel = this.FindControl<StackPanel>("actionsPanel");
		if (actionsPanel == null || ViewModel == null) return;

		actionsPanel.Children.Clear();

		var selectedItems = GetSelectedItems();

		// Добавляем кнопки действий
		foreach (var action in ViewModel.NodeActions ?? Enumerable.Empty<IJournalAction>())
		{
			var button = new Button
			{
				Content = action.GetTitle(selectedItems),
				IsEnabled = action.GetSensitivity(selectedItems),
				IsVisible = action.GetVisibility(selectedItems)
			};

			button.Click += (_, _) =>
			{
				var items = GetSelectedItems();
				action.ExecuteAction?.Invoke(items);
			};

			actionsPanel.Children.Add(button);
		}
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
			ViewModel.UpdateJournalActions -= UpdateButtonActions;
		}
	}
}

