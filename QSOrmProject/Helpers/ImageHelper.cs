using System;
using System.IO;

namespace QS.Helpers
{
	public static class ImageHelper
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Функция загружает изображение из файла любого формата поддерживаемого PixBuf
		/// и возвращает массив байт файла в формете JPG.
		/// </summary>
		/// <returns>The image to jpg bytes.</returns>
		/// <param name="filename">Filename.</param>
		public static byte[] LoadImageToJpgBytes(string filename)
		{
			byte[] imageFile;
			using(FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{

				if(filename.ToLower().EndsWith(".jpg")) {
					using(MemoryStream ms = new MemoryStream()) {
						fs.CopyTo(ms);
						imageFile = ms.ToArray();
					}
				} else {
					logger.Info("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf(fs);
					imageFile = image.SaveToBuffer("jpeg");
				}
			}

			return imageFile;
		}
	}
}
