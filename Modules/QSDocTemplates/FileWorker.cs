using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QSDocTemplates
{
	public class FileWorker : IDisposable
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private List<OpenedFile> openedFiles = new List<OpenedFile> ();

		public FileWorker()
		{
		}

		public void OpenInOffice(IDocTemplate template, bool readOnly)
		{
			logger.Info ("Сохраняем временный файл...");
			OdtWorks odt;
			odt = new OdtWorks (template.File);
			odt.DocParser = template.DocParser;
			odt.DocParser.UpdateFields();
			odt.UpdateFields ();
			var file = odt.GetArray ();
			odt.Close ();

			var opened = openedFiles.FirstOrDefault(x => x.Template == template);

			if (opened == null)
			{
				opened = new OpenedFile(template);
				openedFiles.Add(opened);
			}
			else
				opened.StopWatch();

			if (File.Exists (opened.TempFilePath))
				File.SetAttributes (opened.TempFilePath, FileAttributes.Normal);

			File.WriteAllBytes (opened.TempFilePath, file);

			if (readOnly)
				File.SetAttributes(opened.TempFilePath, FileAttributes.ReadOnly);
			else
				opened.StartWatch();

			logger.Info ("Открываем файл во внешнем приложении...");
			System.Diagnostics.Process.Start (opened.TempFilePath);
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

	internal class OpenedFile
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public IDocTemplate Template;
		public FileSystemWatcher Watcher;
		public string TempFilePath;
		private string tempDir;
		private string tempFile;

		public OpenedFile(IDocTemplate template)
		{
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
				Watcher.Filter = tempFile;

				Watcher.Changed += OnFileChangedByUser;
			}

			Watcher.EnableRaisingEvents = true;
		}

		public void StopWatch()
		{
			if (Watcher != null)
				Watcher.EnableRaisingEvents = false;
		}

		private void OnFileChangedByUser (object source, FileSystemEventArgs e)
		{
			logger.Info ("Файл <{0}> изменен, обновляем...", e.Name);
			try 
			{
				byte[] file;
				using (FileStream fs = new FileStream (e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
					using (MemoryStream ms = new MemoryStream ()) {
						fs.CopyTo (ms);
						file = ms.ToArray ();
					}
				}

				Template.File = file;

			} catch (Exception ex) {
				logger.Warn (ex, "Ошибка при чтении файла!");
			}
		}
	}
}

