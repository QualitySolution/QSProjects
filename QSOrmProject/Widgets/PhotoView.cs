using System;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;
using QS.Helpers;
using Window = Gtk.Window;

namespace QSOrmProject
{
	[ToolboxItem(true)]
	[Category("Gamma Widgets")]
	public partial class PhotoView : Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public BindingControler<PhotoView> Binding { get; private set; }

		bool canPrint;
		public bool CanPrint {
			get => canPrint;
			set {
				canPrint = value;
				btnPrint.Sensitive = value && ImageFile != null;
			}
		}

		public event EventHandler ImageChanged;

		public PhotoView()
		{
			Binding = new BindingControler<PhotoView>(
				this,
				new Expression<Func<PhotoView, object>>[] {
					w => w.ImageFile
				}
			);
			this.Build();
			CanPrint = false;
		}

		public Func<string> GetSaveFileName;

		public byte[] ImageFile {
			get => imageviewerPhoto.ImageFile;
			set {
				if(value == imageviewerPhoto.ImageFile)
					return;
				imageviewerPhoto.ImageFile = value;
				buttonSavePhoto.Sensitive = value != null;
				btnPrint.Sensitive = value != null && CanPrint;
				OnImageChanged();
			}
		}

		protected void OnButtonLoadPhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog(
				"Выберите фото для загрузки...",
				(Window)this.Toplevel,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept
			);

			FileFilter Filter = new FileFilter();
			Filter.AddPixbufFormats();
			Filter.Name = "Все изображения";
			Chooser.AddFilter(Filter);

			if((ResponseType)Chooser.Run() == ResponseType.Accept) {
				Chooser.Hide();
				logger.Info("Загрузка фотографии...");

				ImageFile = ImageHelper.LoadImageToJpgBytes(Chooser.Filename);

				buttonSavePhoto.Sensitive = true;
				btnPrint.Sensitive = CanPrint;
				logger.Info("Ok");
			}
			Chooser.Destroy();
		}

		protected void OnButtonSavePhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog fc = new FileChooserDialog(
				"Укажите файл для сохранения фотографии",
				(Window)this.Toplevel,
				FileChooserAction.Save,
				"Отмена", ResponseType.Cancel,
				"Сохранить", ResponseType.Accept
			) {
				CurrentName = (GetSaveFileName != null ? GetSaveFileName() : "фото") + ".jpg"
			};
			fc.Show();
			if(fc.Run() == (int)ResponseType.Accept) {
				fc.Hide();
				FileStream fs = new FileStream(fc.Filename, FileMode.Create, FileAccess.Write);
				fs.Write(ImageFile, 0, ImageFile.Length);
				fs.Close();
			}
			fc.Destroy();
		}

		protected void OnImageviewerPhotoButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(((Gdk.EventButton)args.Event).Type == Gdk.EventType.TwoButtonPress) {
				string filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_img.jpg");
				FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				fs.Write(ImageFile, 0, ImageFile.Length);
				fs.Close();
				System.Diagnostics.Process.Start(filePath);
			}
		}

		protected void OnImageChanged()
		{
			Binding.FireChange(w => w.ImageFile);

			ImageChanged?.Invoke(this, EventArgs.Empty);
		}

		protected void OnBtnPrintClicked(object sender, EventArgs e)
		{
			if(CanPrint) {
				var printOperation = new PrintOperation {
					Unit = Unit.Points,
					UseFullPage = true,
					DefaultPageSetup = new PageSetup()
				};
				printOperation.DefaultPageSetup.Orientation = PageOrientation.Portrait;

				printOperation.BeginPrint += (s, ea) => printOperation.NPages = 1;
				printOperation.DrawPage += HandlePrintDrawPage;

				printOperation.Run(PrintOperationAction.PrintDialog, null);
			}
		}

		void HandlePrintDrawPage(object o, DrawPageArgs a)
		{
			using(PrintContext context = a.Context) {
				using(var pixBuf = imageviewerPhoto.Pixbuf) {
					Cairo.Context cr = context.CairoContext;

					double scale = 1;
					if(pixBuf.Height * context.Width / pixBuf.Width <= context.Height)
						scale = context.Width / pixBuf.Width;
					if(pixBuf.Width * context.Height / pixBuf.Height <= context.Width)
						scale = context.Height / pixBuf.Height;

					cr.Scale(scale, scale);

					cr.MoveTo(0, 0);
					Gdk.CairoHelper.SetSourcePixbuf(cr, pixBuf, 0, 0);
					cr.Paint();

					((IDisposable)cr).Dispose();
				}
			}
		}
	}
}