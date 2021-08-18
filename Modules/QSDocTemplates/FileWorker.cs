using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gtk;
using QS.DocTemplates;

namespace QSDocTemplates
{
	public class FileWorker : IDisposable
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private List<OpenedFile> openedFiles = new List<OpenedFile> ();

		public event EventHandler<FileUpdatedEventArgs> FileUpdated;

		public FileWorker()
		{
		}

		internal void OnFileUpdated(IDocTemplate template, string path)
		{
			if(FileUpdated != null)
			{
				var arg = new FileUpdatedEventArgs
				{
					Template = template,
					TempFilePath = path
				};
				FileUpdated(this, arg);
			}
		}

		public void OpenInOffice(IDocTemplate template, bool readOnly, FileEditMode mode, bool silentPrint = false,bool IsSavedFile = false)
		{
			int docsToPrint = 0;
			if(silentPrint)
				docsToPrint = 1;
			OpenInOffice(template, readOnly, mode, docsToPrint,IsSavedFile:IsSavedFile);
		}

		/// <summary>
		/// Создание, либо печать документа из шаблона.
		/// </summary>
		/// <param name="template">Шаблон.</param>
		/// <param name="readOnly">Если <c>true</c> то документу будут установлены права только для чтения.</param>
		/// <param name="mode">???</param>
		/// <param name="docsToPrint">Количество копий для печати. Если 0, то будет открыт и не напечатан.</param>
		/// <param name="PrintSettings">Настройки печати</param>
		public void OpenInOffice(IDocTemplate template, bool readOnly, FileEditMode mode, int docsToPrint, PrintSettings PrintSettings = null, bool IsSavedFile = false)
		{
			logger.Info("Сохраняем временный файл...");
			OdtWorks odt;
			odt = new OdtWorks(template.File);
			odt.DocParser = template.DocParser;
			odt.DocParser.UpdateFields();
			odt.UpdateFields();
			if(odt.DocParser.FieldsHasValues && IsSavedFile == false) {
				odt.FillValues();
			}
			var file = odt.GetArray();
			odt.Close();

			var opened = openedFiles.FirstOrDefault(x => x.Template == template);

			if(opened == null) {
				opened = new OpenedFile(this, template, mode);
				openedFiles.Add(opened);
			} else
				opened.StopWatch();

			if(File.Exists(opened.TempFilePath))
				File.SetAttributes(opened.TempFilePath, FileAttributes.Normal);

			try {
				File.WriteAllBytes(opened.TempFilePath, file);
			} catch(UnauthorizedAccessException) {
				string tempName = opened.TempFilePath;
				FileInfo fi = new FileInfo(opened.TempFilePath);
				int tryesCount = 0;
				bool error = true;

				while(error) {
					tryesCount++;
					tempName = string.Format("{0}{1}({2}){3}",
						fi.Directory + Path.DirectorySeparatorChar.ToString(),
						Path.GetFileNameWithoutExtension(fi.Name),
						tryesCount,
						fi.Extension);
					try {
						File.WriteAllBytes(tempName, file);
						error = false;
					} catch(UnauthorizedAccessException) {
						error = true;
					}
				}
				opened.TempFilePath = tempName;
			}

			if(readOnly)
				File.SetAttributes(opened.TempFilePath, FileAttributes.ReadOnly);
			else
				opened.StartWatch();


			if(docsToPrint > 0 && PrintSettings != null && PrintSettings.Printer == null) {
				//значит что нажата кнопка "отмена" в диалоге выбора принтера
				return;
			}

			if(docsToPrint > 0 && PrintSettings != null && PrintSettings.Printer != null) {
				string officeName = "soffice";
				string args = "--pt \"" + PrintSettings.Printer + "\" \"" + opened.TempFilePath + "\"";
				for(int i = 1; i < docsToPrint; i++) {
					args += " \"" + opened.TempFilePath + "\"";
				}

				logger.Info("Печатаем файл...");
				System.Diagnostics.Process.Start(officeName, args).WaitForExit();
			} else if(docsToPrint > 0 && PrintSettings == null) {
				//оставлена старая реализация метода (1 докум на принтере по умолч)
				string officeName = "soffice";
				string args = "-p \"" + opened.TempFilePath + "\"";

				logger.Info("Печатаем файл...");
				System.Diagnostics.Process.Start(officeName, args).WaitForExit();
			} else {
				string officeName = "soffice";
				string args = "-o \"" + opened.TempFilePath + "\"";

				logger.Info("Открываем файл во внешнем приложении...");
				System.Diagnostics.Process.Start(officeName, args).WaitForExit();
			}
		}

		/// <summary>
		/// Подготавливает ODT документ к выгрузке
		/// </summary>
		/// <param name="template">Шаблон</param>
		/// <param name="mode">Режим редактирования файла</param>
		/// <returns>Строку с именем файла</returns>
		public string PrepareToExportODT(IDocTemplate template, FileEditMode mode)
		{
			logger.Info("Сохраняем временный файл...");
			OdtWorks odt;
			odt = new OdtWorks(template.File);
			odt.DocParser = template.DocParser;
			odt.DocParser.UpdateFields();
			odt.UpdateFields();
			if (odt.DocParser.FieldsHasValues)
			{
				odt.FillValues();
			}
			var file = odt.GetArray();
			odt.Close();

			var opened = openedFiles.FirstOrDefault(x => x.Template == template);

			if (opened == null)
			{
				opened = new OpenedFile(this, template, mode);
				openedFiles.Add(opened);
			}
			else
				opened.StopWatch();

			if (File.Exists(opened.TempFilePath))
				File.SetAttributes(opened.TempFilePath, FileAttributes.Normal);

			try
			{
				File.WriteAllBytes(opened.TempFilePath, file);
			}
			catch (UnauthorizedAccessException)
			{
				string tempName = opened.TempFilePath;
				FileInfo fi = new FileInfo(opened.TempFilePath);
				int tryesCount = 0;
				bool error = true;

				while (error)
				{
					tryesCount++;
					tempName = string.Format("{0}{1}({2}){3}",
						fi.Directory + Path.DirectorySeparatorChar.ToString(),
						Path.GetFileNameWithoutExtension(fi.Name),
						tryesCount,
						fi.Extension);
					try
					{
						File.WriteAllBytes(tempName, file);
						error = false;
					}
					catch (UnauthorizedAccessException)
					{
						error = true;
					}
				}
				opened.TempFilePath = tempName;
			}

			File.SetAttributes(opened.TempFilePath, FileAttributes.ReadOnly);

			return opened.TempFilePath;
		}

		#region IDisposable implementation
		public void Dispose()
		{
			foreach(var file in openedFiles)
			{
				if(file.Watcher != null)
					file.Watcher.EnableRaisingEvents = false;
				
				if(File.Exists (file.TempFilePath))
				{
					logger.Info ("Удаляем временный файл {0}", file.TempFilePath);
					//Снимаем установленный атрибут только на чтение если есть.
					File.SetAttributes (file.TempFilePath, FileAttributes.Normal);
					File.Delete (file.TempFilePath);
				}
			}
		}
		#endregion
	}

	public enum FileEditMode{
		Template,
		Document
	}

	internal class OpenedFile
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public IDocTemplate Template;
		public FileSystemWatcher Watcher;
		public string TempFilePath;
		private FileWorker myFileWorker;
		private string tempDir;
		private string tempFile;
		private FileEditMode editMode;
		private bool _isOpenEvent;

		public OpenedFile(FileWorker worker, IDocTemplate template, FileEditMode mode)
		{
			myFileWorker = worker;
			editMode = mode;
			Template = template;
			tempDir = Path.GetTempPath();
			tempFile = template.Name + ".odt";
			TempFilePath = Path.Combine (tempDir, tempFile);
		}

		public void StartWatch ()
		{
			if (Watcher == null)
			{
				Watcher = new FileSystemWatcher();
				Watcher.Path = tempDir;
				Watcher.NotifyFilter = NotifyFilters.LastWrite;
				Watcher.Filter = $".~lock.{tempFile}#";

				Watcher.Changed += OnFileChangedByUser;
            }

            _isOpenEvent = true;
			Watcher.EnableRaisingEvents = true;
		}

		public void StopWatch()
		{
			if (Watcher != null)
				Watcher.EnableRaisingEvents = false;
		}

		private void OnFileChangedByUser (object source, FileSystemEventArgs e)
		{
			if (_isOpenEvent)
			{
				_isOpenEvent = false;

				return;
			}

			logger.Info ("Файл <{0}> изменен, обновляем...", e.Name);
			try 
			{
				byte[] file;
				using (FileStream fs = new FileStream (TempFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
					using (MemoryStream ms = new MemoryStream ()) {
						fs.CopyTo (ms);
						file = ms.ToArray ();
					}
				}

				switch(editMode)
				{
					case FileEditMode.Template: 
						Template.TempalteFile = file;
						break;
					case FileEditMode.Document:
						Template.ChangedDocFile = file;
						break;
				}

				myFileWorker.OnFileUpdated(Template, TempFilePath);

			} catch (Exception ex) {
				logger.Warn (ex, "Ошибка при чтении файла!");
			}
		}
	}

	public class FileUpdatedEventArgs : EventArgs
	{
		public IDocTemplate Template;
		public string TempFilePath;
	}
}

