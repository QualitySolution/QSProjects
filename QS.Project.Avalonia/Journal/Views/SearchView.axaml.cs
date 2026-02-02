using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using QS.Project.Search;

namespace QS.Journal.Views;

public partial class SearchView : UserControl
{
	private SearchViewModel? viewModel;
	private DispatcherTimer? searchTimer;
	
	/// <summary>
	/// Задержка в передачи запроса на поиск во view model.
	/// Измеряется в миллисекундах.
	/// </summary>
	public static uint QueryDelay = 300;

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
		var entry1 = this.FindControl<TextBox>("entrySearch");
		var entry2 = this.FindControl<TextBox>("entrySearch2");
		var entry3 = this.FindControl<TextBox>("entrySearch3");
		var entry4 = this.FindControl<TextBox>("entrySearch4");

		if (entry1 != null)
			entry1.TextChanged += OnSearchTextChanged;
		if (entry2 != null)
			entry2.TextChanged += OnSearchTextChanged;
		if (entry3 != null)
			entry3.TextChanged += OnSearchTextChanged;
		if (entry4 != null)
			entry4.TextChanged += OnSearchTextChanged;

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
				UpdateSearch();
			};
		}
	}

	private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
	{
		if (searchTimer != null)
		{
			searchTimer.Stop();
			searchTimer.Start();
		}
		else
		{
			UpdateSearch();
		}
	}

	private void UpdateSearch()
	{
		if (viewModel == null) return;

		var entry1 = this.FindControl<TextBox>("entrySearch");
		var entry2 = this.FindControl<TextBox>("entrySearch2");
		var entry3 = this.FindControl<TextBox>("entrySearch3");
		var entry4 = this.FindControl<TextBox>("entrySearch4");

		var allFields = new[]
		{
			entry1?.Text ?? "",
			entry2?.Text ?? "",
			entry3?.Text ?? "",
			entry4?.Text ?? ""
		};

		viewModel.SearchValues = allFields.Where(x => !string.IsNullOrEmpty(x)).ToArray();
	}
}

