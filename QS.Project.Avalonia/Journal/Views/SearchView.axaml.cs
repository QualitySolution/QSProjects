using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using QS.Journal.Search;

namespace QS.Journal.Views;

public partial class SearchView : UserControl
{
	private SearchViewModel? viewModel;
	private DispatcherTimer? searchTimer;
	
	#region Настройка
	/// <summary>
	/// Задержка в передачи запроса на поиск во view model.
	/// Измеряется в миллисекундах.
	/// </summary>
	public static uint QueryDelay = 0;
	#endregion

	public SearchView()
	{
		InitializeComponent();
	}

	public SearchView(SearchViewModel viewModel) : this()
	{
		this.viewModel = viewModel;
		DataContext = viewModel;
		ConfigureView();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	private void ConfigureView()
	{
		var entrySearchControl = this.FindControl<TextBox>("entrySearch");
		var buttonSearchClearControl = this.FindControl<Button>("buttonSearchClear");

		if (entrySearchControl != null)
		{
			entrySearchControl.TextChanged += EntrySearch_TextChanged;
			entrySearchControl.KeyDown += EntrySearch_KeyDown;
		}

		if (buttonSearchClearControl != null)
		{
			buttonSearchClearControl.Click += ButtonSearchClear_Click;
		}

		// Настраиваем таймер для отложенного поиска
		if (QueryDelay > 0)
		{
			searchTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(QueryDelay)
			};
			searchTimer.Tick += (_, _) =>
			{
				searchTimer.Stop();
				RunSearch();
			};
		}
	}

	private void EntrySearch_TextChanged(object? sender, TextChangedEventArgs e)
	{
		if (QueryDelay != 0 && searchTimer != null)
		{
			searchTimer.Stop();
			searchTimer.Start();
		}
		else
		{
			RunSearch();
		}
	}

	private void EntrySearch_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return || e.Key == Key.Enter)
		{
			searchTimer?.Stop();
			RunSearch();
		}
	}

	private void RunSearch()
	{
		if (viewModel == null) return;

		var entrySearchControl = this.FindControl<TextBox>("entrySearch");
		if (entrySearchControl != null)
		{
			var allFields = entrySearchControl.Text?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
			viewModel.SearchValues = allFields;
		}
	}

	private void ButtonSearchClear_Click(object? sender, RoutedEventArgs e)
	{
		if (viewModel != null)
		{
			viewModel.SearchValues = Array.Empty<string>();
			var entrySearchControl = this.FindControl<TextBox>("entrySearch");
			if (entrySearchControl != null)
			{
				entrySearchControl.Text = string.Empty;
			}
		}
	}
}

