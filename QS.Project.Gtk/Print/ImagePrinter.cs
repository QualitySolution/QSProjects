using System;
using System.Collections.Generic;
using System.Linq;
using Gdk;
using Gtk;

namespace QS.Print
{
	public static class ImagePrinter
	{
		public static PrintSettings PrintSettings { get; set; }
		public static event EventHandler DocumentsPrinted;
		public static event EventHandler PrintingCanceled;

		static PrintStatus? status = null;
		static PrintOperation printOperation;
		static PrintOperationAction printOperationAction = PrintOperationAction.PrintDialog;
		static IPrintableImage currentImage;

		/// <summary>
		/// Метод необходимо вызвать при старте проекта для возможности массовой печати изображений.
		/// </summary>
		public static void InitPrinter()
		{
			DocumentPrinters.ImagePrinter = new ImgPrinter();
		}

		class ImgPrinter : IImagePrinter
		{
			public void Print(IPrintableDocument[] documents, PrintSettings printSettings = null)
			{
				CreatePrinter();
				PrintSettings = printSettings;
				PrintAll(documents.Where(x => x.PrintType == PrinterType.Image).Cast<IPrintableImage>());
			}
		}

		static void CreatePrinter()
		{
			printOperation = new PrintOperation {
				Unit = Unit.Points,
				UseFullPage = true,
				ShowProgress = true
			};
			printOperation.BeginPrint += (s, ea) => printOperation.NPages = 1;
			printOperation.DrawPage += PrintOperation_DrawPage;
		}

		static void PrintOperation_DrawPage(object o, DrawPageArgs args)
		{
			using(PrintContext context = args.Context) {
				using(var pixBuf = currentImage.GetPixbuf()) {
					Cairo.Context cr = context.CairoContext;
					double scale = 1;
					if(pixBuf.Height * context.Width / pixBuf.Width <= context.Height)
						scale = context.Width / pixBuf.Width;
					if(pixBuf.Width * context.Height / pixBuf.Height <= context.Width)
						scale = context.Height / pixBuf.Height;

					cr.Scale(scale, scale);

					cr.MoveTo(0, 0);
					CairoHelper.SetSourcePixbuf(cr, pixBuf, 0, 0);
					cr.Paint();

					((IDisposable)cr).Dispose();
				}
			}
		}

		static public bool Print(IPrintableImage image)
		{
			currentImage = image;

			if(PrintSettings != null)
				printOperationAction = PrintOperationAction.Print;
			else
				PrintSettings = new PrintSettings();

			printOperation.PrintSettings = PrintSettings;
			PrintSettings.NCopies = currentImage.CopiesToPrint;

			if(currentImage.Orientation == DocumentOrientation.Landscape)
				printOperation.RequestPageSetup += PrintOperation_RequestPageSetup;

			PrintSettings.Orientation = currentImage.Orientation == DocumentOrientation.Portrait ? PageOrientation.Portrait : PageOrientation.Landscape;

			printOperation.Run(printOperationAction, null);
			status = printOperation.Status;
			//если отмена из диалога печати
			if(status.HasValue && status.Value == PrintStatus.FinishedAborted) {
				PrintingCanceled?.Invoke(printOperation, new EventArgs());
				return false;
			}

			PrintSettings = printOperation.PrintSettings;
			return true;
		}

		static void PrintOperation_RequestPageSetup(object o, RequestPageSetupArgs args)
		{
			args.Setup.Orientation = currentImage.Orientation == DocumentOrientation.Portrait ? PageOrientation.Portrait : PageOrientation.Landscape;
		}

		static public void PrintAll(IEnumerable<IPrintableImage> images)
		{
			foreach(var image in images) {
				if(!Print(image))
					break;
			}
		}
	}

	public interface IPrintableImage : IPrintableDocument
	{
		Pixbuf GetPixbuf();
	}
}