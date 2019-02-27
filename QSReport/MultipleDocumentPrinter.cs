using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using QS.DomainModel.Entity;
using QS.Print;

namespace QSReport
{
	public class MultipleDocumentPrinter
	{
		public GenericObservableList<SelectablePrintDocument> PrintableDocuments { get; set; } = new GenericObservableList<SelectablePrintDocument>();
		public event EventHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;
		public PrintSettings PrinterSettings { get; set; }

		public void PrintSelectedDocuments()
		{
			var prnbleDocs = PrintableDocuments.Where(d => d.Selected);
			if(!prnbleDocs.Any())
				return;

			List<IPrintableDocument> rdlToPrinter = new List<IPrintableDocument>();
			List<IPrintableDocument> odtToPrinter = new List<IPrintableDocument>();
			List<IPrintableDocument> imgToPrinter = new List<IPrintableDocument>();
			foreach(var document in prnbleDocs) {
				document.Document.CopiesToPrint = document.Copies;
				switch(document.Document.PrintType) {
					case PrinterType.ODT:
						document.Document.CopiesToPrint = document.Copies;
						odtToPrinter.Add(document.Document);
						break;
					case PrinterType.RDL:
						for(int i = 0; i < document.Copies; i++)
							rdlToPrinter.Add(document.Document);
						break;
					case PrinterType.Image:
						for(int i = 0; i < document.Copies; i++)
							imgToPrinter.Add(document.Document);
						break;
					default:
						throw new NotImplementedException("Печать документа не поддерживается");
				}
			}
			var printer = new DocumentPrinter(PrinterSettings);
			printer.DocumentsPrinted += (sender, e) => DocumentsPrinted?.Invoke(sender, e);
			printer.PrintingCanceled += (sender, e) => PrintingCanceled?.Invoke(sender, e);
			printer.PrintAll(rdlToPrinter);
			DocumentPrinters.ImagePrinter?.Print(imgToPrinter.ToArray(), printer.PrintSettings);
			DocumentPrinters.OdtDocPrinter?.Print(odtToPrinter.ToArray(), printer.PrintSettings);
			PrinterSettings = printer.PrintSettings;
		}

		public void PrintDocument(SelectablePrintDocument doc)
		{
			PrintableDocuments?.Clear();
			PrintableDocuments.Add(doc);
			PrintSelectedDocuments();
		}
	}

	public class SelectablePrintDocument : PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value, () => Selected);
		}

		public IPrintableDocument Document { get; set; }

		public PageOrientation GetPageOrientation() => Document.Orientation == DocumentOrientation.Portrait ? PageOrientation.Portrait : PageOrientation.Landscape;

		private int copies;
		public int Copies {
			get => copies;
			set => SetField(ref copies, value < 1 ? 1 : value, () => Copies);
		}

		public SelectablePrintDocument(IPrintableDocument document)
		{
			Document = document;
			Copies = document.CopiesToPrint;
		}
	}
}