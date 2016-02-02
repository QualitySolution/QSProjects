using System;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem(true)]
	[Category ("Gamma Widgets")]
	public partial class PhotoView : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public BindingControler<PhotoView> Binding { get; private set;}

		public event EventHandler ImageChanged;

		public PhotoView()
		{
			Binding = new BindingControler<PhotoView> (this, new Expression<Func<PhotoView, object>>[] {
				(w => w.ImageFile)
			});
			this.Build();
		}

		public Func<string> GetSaveFileName;

		public byte[] ImageFile{
			get{
				return imageviewerPhoto.ImageFile;
			}
			set {
				if (value == imageviewerPhoto.ImageFile)
					return;
				imageviewerPhoto.ImageFile = value;
				buttonSavePhoto.Sensitive = value != null;
				OnImageChanged();
			}
		}

		protected void OnButtonLoadPhotoClicked (object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog ("Выберите фото для загрузки...", 
				(Window)this.Toplevel,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept);

			FileFilter Filter = new FileFilter ();
			Filter.AddPixbufFormats ();
			Filter.Name = "Все изображения";
			Chooser.AddFilter (Filter);

			if ((ResponseType)Chooser.Run () == ResponseType.Accept) {
				Chooser.Hide ();
				logger.Info ("Загрузка фотографии...");

				FileStream fs = new FileStream (Chooser.Filename, FileMode.Open, FileAccess.Read);
				if (Chooser.Filename.ToLower ().EndsWith (".jpg")) {
					using (MemoryStream ms = new MemoryStream ()) {
						fs.CopyTo (ms);
						ImageFile = ms.ToArray ();
					}
				} else {
					logger.Info ("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf (fs);
					ImageFile = image.SaveToBuffer ("jpeg");
				}
				fs.Close ();
				buttonSavePhoto.Sensitive = true;
				logger.Info ("Ok");
			}
			Chooser.Destroy ();
		}

		protected void OnButtonSavePhotoClicked (object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog ("Укажите файл для сохранения фотографии",
					(Window)this.Toplevel,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.CurrentName = GetSaveFileName != null ? GetSaveFileName() : "фото"  + ".jpg";
			fc.Show (); 
			if (fc.Run () == (int)ResponseType.Accept) {
				fc.Hide ();
				FileStream fs = new FileStream (fc.Filename, FileMode.Create, FileAccess.Write);
				fs.Write (ImageFile, 0, ImageFile.Length);
				fs.Close ();
			}
			fc.Destroy ();
		}

		protected void OnImageviewerPhotoButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if (((Gdk.EventButton)args.Event).Type == Gdk.EventType.TwoButtonPress) {
				string filePath = System.IO.Path.Combine (System.IO.Path.GetTempPath (), "temp_img.jpg");
				FileStream fs = new FileStream (filePath, FileMode.Create, FileAccess.Write);
				fs.Write (ImageFile, 0, ImageFile.Length);
				fs.Close ();
				System.Diagnostics.Process.Start (filePath);
			}
		}

		protected void OnImageChanged()
		{
			Binding.FireChange(w => w.ImageFile);

			if(ImageChanged != null)
			{
				ImageChanged(this, EventArgs.Empty);
			}
		}
	}
}

