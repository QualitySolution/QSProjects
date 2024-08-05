using Gtk;
using QS.DocTemplates;
using System;
using System.IO;
using System.Linq;

namespace QSDocTemplates {
	public class FileWorker : QS.DocTemplates.FileWorker 
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

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

			var opened = OpenedFiles.FirstOrDefault(x => x.Template == template);

			if(opened == null) {
				opened = new OpenedFile(this, template, mode);
				OpenedFiles.Add(opened);
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
	}
}

