using System;
using System.IO;
using QSScan;
using NLog;
using Gdk;

namespace QSAttachment
{
	public partial class GetFromScanner : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		enum FileFormat {
			jpeg,
			pdf
		}

		public byte[] File;

		public string FileName {
			get {
				return entryFileName.Text + labelExtension.LabelProp;
			}
		}

		public GetFromScanner ()
		{
			this.Build ();
			TestCanSave ();
		}

		protected void OnCombobox1Changed(object sender, EventArgs e)
		{
			switch ((FileFormat)comboFormat.Active) {
			case FileFormat.jpeg:
				labelExtension.LabelProp = ".jpg";
				break;
			case FileFormat.pdf:
				labelExtension.LabelProp = ".pdf";
				break;
			default:
				break;
			}
		}

		protected void OnEntryFileNameChanged(object sender, EventArgs e)
		{
			TestCanSave ();
		}
			
		protected	void TestCanSave ()
		{
			bool Nameok = (entryFileName.Text != "");
			bool Fileok = vimageslist1.Images.Count > 0;

			labelInfo.Visible = comboFormat.Active == (int)FileFormat.jpeg && vimageslist1.Images.Count > 1;

			buttonOk.Sensitive = Nameok && Fileok;
		}

		protected void OnButtonScanClicked(object sender, EventArgs e)
		{
			ScanWorks scan = null;
			try
			{
				logger.Debug(Environment.OSVersion.Platform.ToString());
				logger.Debug("init scan");
				scan = new ScanWorks();
				logger.Info ("Получение изображений со сканера...");
				//MainClass.WaitRedraw();
				logger.Debug("run scanner");

				scan.ImageTransfer += delegate(object s, ScanWorks.ImageTransferEventArgs arg) 
				{
					if(arg.AllImages > 0)
						progressScan.Adjustment.Upper = arg.AllImages;
					logger.Debug("ImageTransfer event");

					vimageslist1.Images.Add (arg.Image);

					if(arg.AllImages > 0)
						progressScan.Adjustment.Value++;
					else
						progressScan.Pulse();
					//MainClass.WaitRedraw();
				};

				scan.GetImages(false);

				progressScan.Fraction = 0;
				vimageslist1.UpdateList ();
				logger.Info ("Ок");
			}
			catch (Exception ex)
			{
				logger.ErrorException("Ошибка в работе со сканером!", ex);
				throw ex;
			}
			finally
			{
				if(scan != null)
					scan.Close ();
			}

		}

		protected void OnButtonOkClicked(object sender, EventArgs e)
		{
			switch ((FileFormat)comboFormat.Active) {
			case FileFormat.jpeg:
				SaveJPG ();
				break;
			case FileFormat.pdf:
				SavePDF ();
				break;
			default:
				break;
			}
		}

		void SavePDF()
		{
			iTextSharp.text.Document document = new iTextSharp.text.Document();
			using (var stream = new MemoryStream ())
			{
				iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
				document.Open();
				foreach(Pixbuf pix in vimageslist1.Images)
				{
					if(pix.Width > pix.Height)
						document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
					else 
						document.SetPageSize(iTextSharp.text.PageSize.A4);
					document.NewPage();
					var image = iTextSharp.text.Image.GetInstance(pix.SaveToBuffer ("jpeg"));
					image.SetAbsolutePosition(0,0);
					image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
					document.Add(image);
				}
				document.Close();
				stream.Position = 0;
				File = stream.ToArray ();
			}
		}

		void SaveJPG()
		{
			File = vimageslist1.Images [0].SaveToBuffer ("jpeg");
		}
	}
}

