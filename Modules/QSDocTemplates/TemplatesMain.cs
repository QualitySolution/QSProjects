using System;
using System.IO;
using Gtk;

namespace QSDocTemplates
{
	public static class TemplatesMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static byte[] GetEmptyTemplate()
		{
			using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("QSDocTemplates.Patterns.empty.odt")) {
				byte[] buffer = new byte[stream.Length];
				stream.Read(buffer, 0, buffer.Length);
				return buffer;
			}
		}

		public static byte[] GetTemplateFromDisk()
		{
			//Читаем файл документа
			FileChooserDialog Chooser = new FileChooserDialog ("Выберите шаблон документа...",
				null,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Выбрать", ResponseType.Accept);
			FileFilter Filter = new FileFilter ();
			Filter.Name = "ODT документы и OTT шаблоны";
			Filter.AddMimeType ("application/vnd.oasis.opendocument.text");
			Filter.AddMimeType ("application/vnd.oasis.opendocument.text-template");
			Filter.AddPattern ("*.odt");
			Filter.AddPattern ("*.ott");
			Chooser.AddFilter (Filter);

			Filter = new FileFilter ();
			Filter.Name = "Все файлы";
			Filter.AddPattern ("*.*");
			Chooser.AddFilter (Filter);

			byte[] file = null;

			if ((ResponseType)Chooser.Run () == ResponseType.Accept) {
				Chooser.Hide ();
				logger.Info ("Чтение файла...");

				file = File.ReadAllBytes(Chooser.Filename);

				logger.Info ("Ok");
			}
			Chooser.Destroy ();

			return file;
		}
	}
}

