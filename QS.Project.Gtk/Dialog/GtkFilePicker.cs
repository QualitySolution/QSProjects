using System;
using System.Linq;
using Gtk;
using QS.Project.Services;
using QS.Tdi;

namespace QS.Dialog.GtkUI
{
	public class GtkFilePicker : IFilePickerService
	{
		private string[] MIMEFilters { get; set; }

		public bool OpenSaveFilePicker(string fileName, out string filePath)
		{
			filePath = string.Empty;
			FileChooserDialog Chooser = new FileChooserDialog
			(
				"Укажите файл для сохранения",
				null,
				FileChooserAction.Save,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept
			);

			Chooser.SetUri(fileName);

			if(MIMEFilters?.Any() ?? false) 
			{
				FileFilter filter = new FileFilter();

				foreach(var item in MIMEFilters)
					filter.AddMimeType(item);

				Chooser.AddFilter(filter);
			}

			if((ResponseType)Chooser.Run() == ResponseType.Accept) 
			{
				Chooser.Hide();
				filePath = Chooser.Filename;
			}

			Chooser.Destroy();
			MIMEFilters = null;

			return !String.IsNullOrWhiteSpace(filePath);
		}

		public bool OpenSaveFilePicker(string fileName, out string filePath, params string[] MIMEFilter)
		{
			MIMEFilters = MIMEFilter;
			return OpenSaveFilePicker(fileName, out filePath);
		}

		public bool OpenSelectFilePicker(out string filePath)
		{
			filePath = string.Empty;

			FileChooserDialog Chooser = new FileChooserDialog
			(
				"Выберите файл для загрузки...",
				null,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept
			);

			if(MIMEFilters?.Any() ?? false) 
			{
				FileFilter filter = new FileFilter();

				foreach(var item in MIMEFilters)
					filter.AddMimeType(item);

				Chooser.AddFilter(filter);
			}

			if((ResponseType)Chooser.Run() == ResponseType.Accept) 
			{
				Chooser.Hide();
				filePath = Chooser.Filename;
			}

			Chooser.Destroy();
			MIMEFilters = null;

			return !String.IsNullOrWhiteSpace(filePath);
		}

		public bool OpenSelectFilePicker(out string[] filePaths)
		{
			filePaths = Array.Empty<string>();
			FileChooserDialog Chooser = new FileChooserDialog
			(
				"Выберите файл для загрузки...",
				null,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept
			)
			{
				SelectMultiple = true
			};

			if(MIMEFilters?.Any() ?? false)
			{
				FileFilter filter = new FileFilter();

				foreach(var item in MIMEFilters)
					filter.AddMimeType(item);

				Chooser.AddFilter(filter);
			}

			if((ResponseType)Chooser.Run() == ResponseType.Accept)
			{
				Chooser.Hide();
				filePaths = Chooser.Filenames;
			}

			Chooser.Destroy();
			MIMEFilters = null;

			return filePaths.Any() && filePaths.FirstOrDefault(String.IsNullOrWhiteSpace) == null;
		}

		public bool OpenSelectFilePicker(out string filePath, params string[] MIMEFilter)
		{
			MIMEFilters = MIMEFilter;
			return OpenSelectFilePicker(out filePath);
		}

		public bool OpenSelectFilePicker(out string[] filePaths, params string[] MIMEFilter)
		{
			MIMEFilters = MIMEFilter;
			return OpenSelectFilePicker(out filePaths);
		}
	}
}
