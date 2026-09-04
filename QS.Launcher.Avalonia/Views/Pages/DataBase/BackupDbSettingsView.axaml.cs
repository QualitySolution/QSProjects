using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using QS.Launcher.ViewModels.PageViewModels.DataBase;

namespace QS.Launcher.Views.Pages.DataBase;

public partial class BackupDbSettingsView : UserControl {
	public BackupDbSettingsView(BackupDbSettingsVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}

	private async void BrowseBackupFile_OnClick(object? sender, RoutedEventArgs e) {
		if(DataContext is not BackupDbSettingsVM vm)
			return;

		var topLevel = TopLevel.GetTopLevel(this);
		if(topLevel == null)
			return;

		var options = new FilePickerSaveOptions {
			Title = "Сохранить резервную копию",
			DefaultExtension = "sql",
			SuggestedFileName = Path.GetFileName(vm.BackupFilePath),
			FileTypeChoices = new[] { new FilePickerFileType("SQL-скрипт") { Patterns = new[] { "*.sql" } } }
		};

		var directory = Path.GetDirectoryName(vm.BackupFilePath);
		if(!string.IsNullOrEmpty(directory)) {
			var folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(directory);
			if(folder != null)
				options.SuggestedStartLocation = folder;
		}

		var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
		if(file != null)
			vm.BackupFilePath = file.Path.LocalPath;
	}
}
