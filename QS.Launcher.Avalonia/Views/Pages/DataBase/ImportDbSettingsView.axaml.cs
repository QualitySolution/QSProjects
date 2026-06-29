using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using QS.Launcher.ViewModels.PageViewModels.DataBase;

namespace QS.Launcher.Views.Pages.DataBase;

public partial class ImportDbSettingsView : UserControl {
	public ImportDbSettingsView(ImportDbSettingsVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}

	private async void BrowseImportFile_OnClick(object? sender, RoutedEventArgs e) {
		if(DataContext is not ImportDbSettingsVM vm)
			return;

		var topLevel = TopLevel.GetTopLevel(this);
		if(topLevel == null)
			return;

		var options = new FilePickerOpenOptions {
			Title = "Выбрать дамп базы данных",
			AllowMultiple = false,
			FileTypeFilter = new[] { new FilePickerFileType("SQL-скрипт") { Patterns = new[] { "*.sql" } } }
		};

		var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
		if(files.Count > 0)
			vm.ImportDumpFilePath = files[0].Path.LocalPath;
	}
}
