using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using QS.Print;
using QS.Report;
using QSProjectsLib;

namespace QSReport
{
	public class DocumentPrinter
	{
		public DocumentPrinter(PrintSettings printSettings = null) => PrintSettings = printSettings;
		public event EventHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;
		PrintStatus? status = null;

		public void Print(IPrintableDocument document)
		{
			PrintAll(new IPrintableDocument[] { document });
		}

		public PrintSettings PrintSettings { get; set; }

		public void PrintAll(IEnumerable<IPrintableDocument> documents)
		{
			if(Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix) {
				PrintSettings = PrintAll_Win_Workaround(documents);
				return;
			}

			PrintOperation printOp;
			printOp = new PrintOperation {
				Unit = Unit.Points,
				UseFullPage = true,
				ShowProgress = true
			};
			var documentsRDL = documents.Where(doc => doc.PrintType == PrinterType.RDL).ToList();

			PrintOperationAction printOperationAction = PrintOperationAction.PrintDialog;
			if(PrintSettings != null) {
				printOp.PrintSettings = PrintSettings;
				printOperationAction = PrintOperationAction.Print;
			}

			BatchRDLRenderer renderer = new BatchRDLRenderer(documentsRDL);

			var result = LongOperationDlg.StartOperation(
				renderer.PrepareDocuments,
				"Подготовка к печати...",
				documentsRDL.Count()
			);
			if(result == LongOperationResult.Canceled) {
				PrintSettings = new PrintSettings();
				status = PrintStatus.FinishedAborted;
				PrintingCanceled?.Invoke(this, new EventArgs());
				return;
			}

			printOp.NPages = renderer.PageCount;
			if(documentsRDL.Any(x => x.Orientation == DocumentOrientation.Landscape))
				printOp.RequestPageSetup += renderer.RequestPageSetup;

			printOp.DrawPage += renderer.DrawPage;
			printOp.EndPrint += (o, args) => {
				args.Args = documentsRDL.ToArray();
				DocumentsPrinted?.Invoke(o, args);
			};
			printOp.Run(printOperationAction, null);
			status = printOp.Status;
			//если отмена из диалога печати
			if(status.HasValue && status.Value == PrintStatus.FinishedAborted)
				PrintingCanceled?.Invoke(this, new EventArgs());
			PrintSettings = printOp.PrintSettings;
		}

		/// <summary>
		/// Альтернативная реализация печати, специально для обхода бага в кайро при поворачивании(со старой реализацией печати пол Linux)
		/// https://bugzilla.mozilla.org/show_bug.cgi?id=1205854#c16
		/// В новой при установке ориентации для каждой странице на большинстве принтеров обрезается часть выходящая за поворот. Хотя ориентация правильная.
		/// На некоторых принтерах например в водовозе, табличка рисуется за вертикалью листа а текст нет.
		/// используется только на винде, в линуксе такой проблемы нет.
		/// </summary>
		private PrintSettings PrintAll_Win_Workaround(IEnumerable<IPrintableDocument> documents)
		{
			PrintOperation printOp = null;

			var documentsRDL_Portrait = documents.Where(doc => doc.PrintType == PrinterType.RDL && doc.Orientation == DocumentOrientation.Portrait).ToList();
			var documentsRDL_Landscape = documents.Where(doc => doc.PrintType == PrinterType.RDL && doc.Orientation == DocumentOrientation.Landscape).ToList();

			if(!documents.Any()) {
				printOp = new PrintOperation();
				printOp.Run(PrintOperationAction.PrintDialog, null);
				return printOp.PrintSettings;
			}

			if(documentsRDL_Portrait.Any()) {
				printOp = new PrintOperation {
					Unit = Unit.Points,
					UseFullPage = true,
					ShowProgress = true,
					DefaultPageSetup = new PageSetup()
				};

				PrintOperationAction printOperationAction = PrintOperationAction.PrintDialog;
				if(PrintSettings != null) {
					printOp.PrintSettings = PrintSettings;
					printOp.PrintSettings.Orientation = PageOrientation.Portrait;
					printOperationAction = PrintOperationAction.Print;
				}

				printOp.DefaultPageSetup.Orientation = PageOrientation.Portrait;

				BatchRDLRenderer renderer = new BatchRDLRenderer(documentsRDL_Portrait);

				var result = LongOperationDlg.StartOperation(
								renderer.PrepareDocuments,
								"Подготовка к печати портретных страниц...",
								documentsRDL_Portrait.Count()
							);
				if(result == LongOperationResult.Canceled) {
					PrintingCanceled?.Invoke(this, new EventArgs());
					status = PrintStatus.FinishedAborted;
					return new PrintSettings();
				}

				printOp.NPages = renderer.PageCount;

				printOp.DrawPage += renderer.DrawPage;
				printOp.EndPrint += (o, args) => {
					args.Args = documentsRDL_Portrait.Concat(documentsRDL_Landscape).ToArray();
					DocumentsPrinted?.Invoke(o, args);
				};
				printOp.Run(printOperationAction, null);
				PrintSettings = printOp.PrintSettings;
				status = printOp.Status;
				//если отмена из диалога печати
				if(status.HasValue && status.Value == PrintStatus.FinishedAborted)
					PrintingCanceled?.Invoke(this, new EventArgs());
			}

			if(documentsRDL_Landscape.Any()) {
				printOp = new PrintOperation {
					Unit = Unit.Points,
					UseFullPage = true,
					ShowProgress = true,
					DefaultPageSetup = new PageSetup()
				};

				//если printSettings == null, то значит, до этого не печатались RDL формата 
				//PageOrientation.Portrait и, соответственно, не показывался PrintDialog и 
				//нужно показать его сейчас.
				PrintOperationAction printOperationAction = PrintOperationAction.PrintDialog;
				if(PrintSettings != null) {
					printOp.PrintSettings = PrintSettings;
					printOp.PrintSettings.Orientation = PageOrientation.Landscape;
					printOperationAction = PrintOperationAction.Print;
				}
				printOp.DefaultPageSetup.Orientation = PageOrientation.Landscape;

				BatchRDLRenderer renderer = new BatchRDLRenderer(documentsRDL_Landscape);

				var result = LongOperationDlg.StartOperation(
					renderer.PrepareDocuments,
					"Подготовка к печати альбомных страниц...",
					documentsRDL_Landscape.Count()
				);
				if(result == LongOperationResult.Canceled) {
					PrintingCanceled?.Invoke(this, new EventArgs());
					status = PrintStatus.FinishedAborted;
					return new PrintSettings();
				}

				printOp.NPages = renderer.PageCount;
				printOp.DrawPage += renderer.DrawPage;
				printOp.EndPrint += (o, args) => {
					args.Args = documentsRDL_Portrait.Concat(documentsRDL_Landscape).ToArray();
					DocumentsPrinted?.Invoke(o, args);
				};
				printOp.Run(printOperationAction, null);
				status = printOp.Status;
				//если отмена из диалога печати
				if(status.HasValue && status.Value == PrintStatus.FinishedAborted)
					PrintingCanceled?.Invoke(this, new EventArgs());
			}
			return printOp.PrintSettings;
		}

		public static QS.Dialog.Gtk.TdiTabBase GetPreviewTab(IPrintableRDLDocument document)
		{
			return new ReportViewDlg(document.GetReportInfo());
		}
	}
}