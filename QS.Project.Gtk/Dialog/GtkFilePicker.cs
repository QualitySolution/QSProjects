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
		
		private FileChooserDialog CreateNewFileChooserDialog(
			out string filePath, string title, FileChooserAction chooserAction, string fileName = null, params object[] buttonData)
		{
			filePath = string.Empty;
			
			var chooser = new FileChooserDialog
			(
				title,
				null,
				chooserAction,
				buttonData
			);

			if(fileName != null)
			{
				chooser.SetUri(fileName);
			}
			
			return chooser;
		}
		
		private void CreateFilters(FileChooserDialog chooser)
		{
			if(!(MIMEFilters?.Any() ?? false)) return;
			
			var filter = new FileFilter();

			foreach(var item in MIMEFilters)
				filter.AddMimeType(item);

			chooser.AddFilter(filter);
		}
		
		private void CheckAcceptButtonClicked(ref string filePath, FileChooserDialog chooser)
		{
			if((ResponseType)chooser.Run() == ResponseType.Accept)
			{
				chooser.Hide();
				filePath = chooser.Filename;
			}
		}
		
		private void CheckAcceptButtonClicked(ref string[] filePaths, FileChooserDialog chooser)
		{
			if((ResponseType)chooser.Run() == ResponseType.Accept)
			{
				chooser.Hide();
				filePaths = chooser.Filenames;
			}
		}
		
		private void DestroyDialog(FileChooserDialog chooser)
		{
			chooser.Destroy();
			MIMEFilters = null;
		}

		public bool OpenSaveFilePicker(string fileName, out string filePath)
		{
			object[] buttonData = {
				"Отмена",
				ResponseType.Cancel,
				"Загрузить",
				ResponseType.Accept
			};
			
			var chooser = CreateNewFileChooserDialog(
				out filePath, "Укажите файл для сохранения", FileChooserAction.Save, fileName, buttonData);

			CreateFilters(chooser);
			CheckAcceptButtonClicked(ref filePath, chooser);
			DestroyDialog(chooser);

			return !string.IsNullOrWhiteSpace(filePath);
		}

		public bool OpenSaveFilePicker(string fileName, out string filePath, params string[] MIMEFilter)
		{
			MIMEFilters = MIMEFilter;
			return OpenSaveFilePicker(fileName, out filePath);
		}

		public bool OpenSelectFilePicker(out string filePath)
		{
			object[] buttonData = {
				"Отмена",
				ResponseType.Cancel,
				"Загрузить",
				ResponseType.Accept
			};
			
			var chooser = CreateNewFileChooserDialog(
				out filePath, "Выберите файл для загрузки...", FileChooserAction.Open, null, buttonData);

			CreateFilters(chooser);
			CheckAcceptButtonClicked(ref filePath, chooser);
			DestroyDialog(chooser);

			return !string.IsNullOrWhiteSpace(filePath);
		}

		public bool OpenSelectFilePicker(out string[] filePaths)
		{
			filePaths = Array.Empty<string>();
			var chooser = new FileChooserDialog
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

			CreateFilters(chooser);
			CheckAcceptButtonClicked(ref filePaths, chooser);
			DestroyDialog(chooser);

			return filePaths.Any() && filePaths.FirstOrDefault(string.IsNullOrWhiteSpace) == null;
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

		public bool OpenAttachFilePicker(out string filePath)
		{
			object[] buttonData = {
				"Отмена",
				ResponseType.Cancel,
				"Прикрепить",
				ResponseType.Accept
			};

			var chooser = CreateNewFileChooserDialog(
				out filePath, "Выберите файл для прикрепления...", FileChooserAction.Open, null, buttonData);

			CreateFilters(chooser);
			CheckAcceptButtonClicked(ref filePath, chooser);
			DestroyDialog(chooser);

			return !string.IsNullOrWhiteSpace(filePath);
		}
	}
}
