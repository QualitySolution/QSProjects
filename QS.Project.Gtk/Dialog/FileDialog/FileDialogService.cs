using Gtk;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QS.Dialog.GtkUI.FileDialog
{
    public class FileDialogService : IFileDialogService
	{
		#region Open file dialog

		public IDialogResult RunOpenFileDialog()
		{
			var defaultSettings = new DialogSettings();
			defaultSettings.FileFilters.Add(new DialogFileFilter("Все файлы (*.*)", "*.*"));
			defaultSettings.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			defaultSettings.SelectMultiple = true;
			defaultSettings.Title = "Открыть";

			return RunOpenFileDialog(defaultSettings);
		}

		public IDialogResult RunOpenFileDialog(DialogSettings dialogSettings)
		{
			switch (dialogSettings.PlatformType)
			{
				case DialogPlatformType.Auto:
					if (IsWindows())
					{
						return RunWindowsOpenFileDialog(dialogSettings);
					}
					else
					{
						return RunGtkOpenFileDialog(dialogSettings);
					}
				case DialogPlatformType.Crossplatform:
					return RunGtkOpenFileDialog(dialogSettings);
				case DialogPlatformType.Windows:
					return RunWindowsOpenFileDialog(dialogSettings);
				default:
					throw new NotSupportedException($"Тип {dialogSettings.PlatformType} не поддерживается");
			}
		}

		private IDialogResult RunWindowsOpenFileDialog(DialogSettings dialogSettings)
		{
			var filter = CreateWindowsFilter(dialogSettings.FileFilters);

			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = dialogSettings.Title;
				openFileDialog.InitialDirectory = dialogSettings.InitialDirectory;
				openFileDialog.Filter = filter;
				openFileDialog.Multiselect = dialogSettings.SelectMultiple;
				foreach (var customDirectory in dialogSettings.CustomDirectories)
				{
					openFileDialog.CustomPlaces.Add(customDirectory);
				}

				IDialogResult result;

				if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					result = new DialogResult(openFileDialog.FileName, openFileDialog.FileNames);
				}
				else
				{
					result = new DialogResult();
				}

				return result;
			}
		}

		private IDialogResult RunGtkOpenFileDialog(DialogSettings dialogSettings)
		{
			FileChooserDialog fileDialog = new FileChooserDialog(dialogSettings.Title, null, FileChooserAction.Open, "Открыть", ResponseType.Accept, "Отмена", ResponseType.Cancel);
			fileDialog.SetCurrentFolder(dialogSettings.InitialDirectory);
			fileDialog.SelectMultiple = dialogSettings.SelectMultiple;

			foreach (var filter in dialogSettings.FileFilters)
			{
				var gtkFilter = new FileFilter();
				gtkFilter.Name = $"{filter.Name} ({string.Join(", ", filter.FilterExtensions)})";
				foreach (var fe in filter.FilterExtensions)
				{
					gtkFilter.AddPattern(fe);
				}
				fileDialog.AddFilter(gtkFilter);
			}

			IDialogResult result;

			var runDialogResult = (ResponseType)fileDialog.Run();
			if (runDialogResult == ResponseType.Accept)
			{
				result = new DialogResult(fileDialog.Filename, fileDialog.Filenames);
			}
			else
			{
				result = new DialogResult();
			}

			fileDialog.Destroy();

			return result;
		}

		#endregion Open file dialog

		#region Save file dialog

		public IDialogResult RunSaveFileDialog()
		{
			var defaultSettings = new DialogSettings();
			defaultSettings.FileFilters.Add(new DialogFileFilter("Все файлы (*.*)", "*.*"));
			defaultSettings.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			defaultSettings.Title = "Сохранить";

			return RunSaveFileDialog(defaultSettings);
		}

		public IDialogResult RunSaveFileDialog(DialogSettings dialogSettings)
		{
			switch (dialogSettings.PlatformType)
			{
				case DialogPlatformType.Auto:
					if (IsWindows())
					{
						return RunWindowsSaveFileDialog(dialogSettings);
					}
					else
					{
						return RunGtkSaveFileDialog(dialogSettings);
					}
				case DialogPlatformType.Crossplatform:
					return RunGtkSaveFileDialog(dialogSettings);
				case DialogPlatformType.Windows:
					return RunWindowsSaveFileDialog(dialogSettings);
				default:
					throw new NotSupportedException($"Тип {dialogSettings.PlatformType} не поддерживается");
			}
		}

		private IDialogResult RunWindowsSaveFileDialog(DialogSettings dialogSettings)
		{
			var filter = CreateWindowsFilter(dialogSettings.FileFilters);

			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Title = dialogSettings.Title;
				saveFileDialog.FileName = dialogSettings.FileName;
				saveFileDialog.InitialDirectory = dialogSettings.InitialDirectory;
				saveFileDialog.Filter = filter;
				saveFileDialog.DefaultExt = dialogSettings.DefaultFileExtention;

				foreach (var customDirectory in dialogSettings.CustomDirectories)
				{
					saveFileDialog.CustomPlaces.Add(customDirectory);
				}

				IDialogResult result;

				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					result = new DialogResult(saveFileDialog.FileName, saveFileDialog.FileNames);
				}
				else
				{
					result = new DialogResult();
				}

				return result;
			}
		}

		private IDialogResult RunGtkSaveFileDialog(DialogSettings dialogSettings)
		{
			FileChooserDialog fileDialog = new FileChooserDialog(dialogSettings.Title, null, FileChooserAction.Save, "Сохранить", ResponseType.Accept, "Отмена", ResponseType.Cancel);
			fileDialog.SetCurrentFolder(dialogSettings.InitialDirectory);
			fileDialog.CurrentName = dialogSettings.FileName;
			foreach (var filter in dialogSettings.FileFilters)
			{
				var gtkFilter = new FileFilter();
				gtkFilter.Name = $"{filter.Name} ({string.Join(", ", filter.FilterExtensions)})";
				foreach (var fe in filter.FilterExtensions)
				{
					gtkFilter.AddPattern(fe);
				}
				fileDialog.AddFilter(gtkFilter);
			}

			IDialogResult result;

			var runDialogResult = (ResponseType)fileDialog.Run();
			if (runDialogResult == ResponseType.Accept)
			{
				result = new DialogResult(fileDialog.Filename, fileDialog.Filenames);
			}
			else
			{
				result = new DialogResult();
			}

			fileDialog.Destroy();

			return result;
		}

		#endregion Save file dialog

		#region Open directory dialog

		public IDialogResult RunOpenDirectoryDialog()
		{
			var defaultSettings = new DialogSettings();
			defaultSettings.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			defaultSettings.Title = "Выбрать каталог";

			return RunOpenDirectoryDialog(defaultSettings);
		}

		public IDialogResult RunOpenDirectoryDialog(DialogSettings dialogSettings)
		{
			switch (dialogSettings.PlatformType)
			{
				case DialogPlatformType.Auto:
					if (IsWindows())
					{
						return RunWindowsOpenDirectoryDialog(dialogSettings);
					}
					else
					{
						return RunGtkOpenDirectoryDialog(dialogSettings);
					}
				case DialogPlatformType.Crossplatform:
					return RunGtkOpenDirectoryDialog(dialogSettings);
				case DialogPlatformType.Windows:
					return RunWindowsOpenDirectoryDialog(dialogSettings);
				default:
					throw new NotSupportedException($"Тип {dialogSettings.PlatformType} не поддерживается");
			}
		}

		private IDialogResult RunWindowsOpenDirectoryDialog(DialogSettings dialogSettings)
		{
			using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
			{
				folderDialog.Description = dialogSettings.Title;
				folderDialog.ShowNewFolderButton = true;
				folderDialog.SelectedPath = dialogSettings.InitialDirectory;
				folderDialog.RootFolder = Environment.SpecialFolder.Desktop;

				IDialogResult result;

				if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					result = new DialogResult(folderDialog.SelectedPath, new[] { folderDialog.SelectedPath });
				}
				else
				{
					result = new DialogResult();
				}

				return result;
			}
		}

		private IDialogResult RunGtkOpenDirectoryDialog(DialogSettings dialogSettings)
		{
			FileChooserDialog fileDialog = new FileChooserDialog(dialogSettings.Title, null, FileChooserAction.SelectFolder, "Выбрать", ResponseType.Accept, "Отмена", ResponseType.Cancel);
			fileDialog.SetCurrentFolder(dialogSettings.InitialDirectory);

			IDialogResult result;

			var runDialogResult = (ResponseType)fileDialog.Run();
			if (runDialogResult == ResponseType.Accept)
			{
				result = new DialogResult(fileDialog.Filename, fileDialog.Filenames);
			}
			else
			{
				result = new DialogResult();
			}

			fileDialog.Destroy();

			return result;
		}

		#endregion Open directory dialog

		private string CreateWindowsFilter(IEnumerable<DialogFileFilter> filters)
        {
			return string.Join("|", filters.Select(x => $"{x.Name}|{string.Join(";", x.FilterExtensions)}"));
		}

		private bool IsWindows()
        {
			return Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix;
		}
	}
}
