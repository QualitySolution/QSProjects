using System;
using System.IO;
using QSScan;
using NLog;
using Gdk;
using QSProjectsLib;

namespace QSAttachment
{
	public partial class GetFromScanner : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ScanWorks scan;

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
			try 
			{
				logger.Debug("init scan");
				scan = new ScanWorks();
				scan.Pulse += OnScanerPulse;
				scan.ImageTransfer += OnScanerImageTransfer;
				foreach(string scannerName in scan.GetScannerList ())
				{
					comboScanner.AppendText (scannerName);
				}
				if(scan.ScannerCount > 0)
					comboScanner.Active = 0;
			} 
			catch (Exception ex) 
			{
				logger.Error (ex, "Не удалось инициализировать библиотеку Saraff.Twain.");
				QSProjectsLib.QSMain.ErrorMessage (this, ex);
				Respond (Gtk.ResponseType.Reject);
			}

			TestCanSave ();
		}

		void OnScanerImageTransfer (object sender, ImageTransferEventArgs e)
		{
			progressScan.Text = "Завершаем загрузку...";
			logger.Debug("ImageTransfer event");

			vimageslist1.Images.Add (e.Image);

			progressScan.Text = "Ок";
			progressScan.Adjustment.Value = progressScan.Adjustment.Upper;
			QSMain.WaitRedraw();
		}

		void OnScanerPulse (object sender, ScanWorksPulseEventArgs e)
		{
			progressScan.Text = e.ProgressText;
			progressScan.Adjustment.Upper = e.ImageByteSize;
			progressScan.Adjustment.Value = e.LoadedByteSize;
			QSProjectsLib.QSMain.WaitRedraw ();
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
			try
			{
				logger.Info ("Получение изображений со сканера {0}...", comboScanner.Active);
				QSMain.WaitRedraw ();
				scan.CurrentScanner = comboScanner.Active;
				logger.Debug("run scanner");

				scan.GetImages();

				progressScan.Fraction = 0;
				progressScan.Text = "";
				vimageslist1.UpdateList ();
				logger.Info ("Ок");
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Ошибка в работе со сканером!");
				throw ex;
			}
			TestCanSave ();
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
				var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
				writer.CloseStream = false;
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

		protected void OnComboScannerChanged(object sender, EventArgs e)
		{
			buttonScan.Sensitive = comboScanner.Active >= 0;
		}

	}
}

