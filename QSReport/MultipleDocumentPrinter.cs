using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using QSOrmProject;

namespace QSReport
{
	public class MultipleDocumentPrinter
	{
		PrintSettings PrintSettings;
		Gtk.PrintOperation Printer;
		bool showDialog = true;

		public GenericObservableList<SelectablePrintDocument> PrintableDocuments { get; set; } = new GenericObservableList<SelectablePrintDocument>();

		public void PrintSelectedDocuments()
		{
			showDialog = true;
			foreach(var document in PrintableDocuments.Where(x => x.Selected)) {
				PrintDoc(document, PageOrientation.Portrait, document.Copies);
			}
		}

		public void PrintDocument(SelectablePrintDocument doc)
		{
			showDialog = true;
			PrintDoc(doc, PageOrientation.Portrait, doc.Copies);
		}

		private void PrintDoc(SelectablePrintDocument doc, PageOrientation orientation, int copies)
		{
			var reportInfo = doc.Document.GetReportInfo();

			var action = showDialog ? PrintOperationAction.PrintDialog : PrintOperationAction.Print;
			showDialog = false;

			Printer = new PrintOperation();
			Printer.Unit = Unit.Points;
			Printer.UseFullPage = true;

			if(PrintSettings == null) {
				Printer.PrintSettings = new PrintSettings();
			} else {
				Printer.PrintSettings = PrintSettings;
			}

			Printer.PrintSettings.Orientation = orientation;

			var rprint = new ReportPrinter(reportInfo);
			rprint.PrepareReport();

			Printer.NPages = rprint.PageCount;
			Printer.PrintSettings.NCopies = copies;
			if(copies > 1)
				Printer.PrintSettings.Collate = true;

			Printer.DrawPage += rprint.DrawPage;
			Printer.Run(action, null);

			PrintSettings = Printer.PrintSettings;
		}
	}

	public class SelectablePrintDocument : PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get { return selected; }
			set { SetField(ref selected, value, () => Selected); }
		}

		public IPrintableDocument Document { get; set; }

		private int copies;
		public int Copies {
			get => copies;
			set {
				var result = value;
				if(result < 1) {
					result = 1;
				}
				SetField(ref copies, result, () => Copies);
			}
		}

		public SelectablePrintDocument(IPrintableDocument document)
		{
			Document = document;
		}

		public SelectablePrintDocument(IPrintableDocument document, int copies)
		{
			Document = document;
			Copies = copies;
		}
	}
}
