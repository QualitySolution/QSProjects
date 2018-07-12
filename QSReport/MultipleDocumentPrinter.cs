using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using QSDocTemplates;
using QSOrmProject;
using System;

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
			List<IPrintableDocument> rdlToPrinter = new List<IPrintableDocument>();
			List<ITemplatePrntDoc> odtToPrinter = new List<ITemplatePrntDoc>();

			foreach(var document in PrintableDocuments.Where(d => d.Selected)) {
				switch(document.Document.PrintType) {
					case PrinterType.ODT:
						if(document.Document is ITemplatePrntDoc) {
							var doc = (document.Document as ITemplatePrntDoc);
							doc.CopiesToPrint = document.Copies;
							odtToPrinter.Add(doc);
						}
						break;
					case PrinterType.RDL:
						for(int i = 0; i < document.Copies; i++)
							rdlToPrinter.Add(document.Document);
						break;
					default:
						throw new NotImplementedException("Печать документа не поддерживается");
				}
			}
			DocumentPrinter.PrintAll(rdlToPrinter);
			TemplatePrinter.PrintSettings = DocumentPrinter.PrintSettings;
			TemplatePrinter.PrintAll(odtToPrinter);
		}

		public void PrintDocument(SelectablePrintDocument doc)
		{
			showDialog = true;
			PrintDoc(doc, PageOrientation.Portrait, doc.Copies);
		}

		private void PrintDoc(SelectablePrintDocument doc, PageOrientation orientation, int copies)
		{
			if(doc == null) {
				return;
			}

			switch(doc.Document.PrintType) {
				case PrinterType.RDL:
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
					break;
				case PrinterType.ODT:
				case PrinterType.None:
				default:
					break;
			}
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

		public PageOrientation GetPageOrientation(){
			switch(Document.Orientation){
				case DocumentOrientation.Landscape:
					return PageOrientation.Landscape;
				default:
					return PageOrientation.Portrait;
			}
		}

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
