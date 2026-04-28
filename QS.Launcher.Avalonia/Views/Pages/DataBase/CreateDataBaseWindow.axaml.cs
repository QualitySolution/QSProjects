using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QS.Launcher.Views.Pages.DataBase;

public partial class CreateDataBaseWindow : Window
{
	private CreateDataBaseVM ViewModel;
	private TaskCompletionSource<(string? dbTitle, string? dbName)> Result;
	public CreateDataBaseWindow(CreateDataBaseVM viewModel)
	{
		DataContext = ViewModel = viewModel;
		InitializeComponent();

		Result = new TaskCompletionSource<(string? dbTitle, string? dbName)>();
		ViewModel.DatabaseCreated += () => {
			Result.TrySetResult((ViewModel.DbTitle, ViewModel.DbName));
			Close();
		};

		this.Closed += (s, e) =>
			Result.TrySetResult((null, null));
	}
	public (string? dbTitle, string? dbName) GetResult(){
		return Result.Task.Result;
	}
}
