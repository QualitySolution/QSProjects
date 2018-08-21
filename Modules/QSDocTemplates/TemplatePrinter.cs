using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using QS.Print;
using QSProjectsLib;

namespace QSDocTemplates
{
	public static class TemplatePrinter
	{
		public static PrintSettings PrintSettings { get; set; }

		public static void Print(IPrintableOdtDocument document)
		{
			PrintAll(new List<IPrintableOdtDocument> { document });
		}

		class OdtPrinter : IOdtDocPrinter
		{
			public void Print(IPrintableDocument[] documents, PrintSettings printSettings = null)
			{
				PrintAll(documents.Where(x => x.PrintType == PrinterType.ODT).Cast<IPrintableOdtDocument>().ToList());
			}
		}

		/// <summary>
		/// Метод необходимо вызвать при старте проекта для возможности массовой печати документов.
		/// </summary>
		public static void InitPrinter()
		{
			DocumentPrinters.OdtDocPrinter = new OdtPrinter();
		}

		public static void PrintAll(IList<IPrintableOdtDocument> documents)
		{
			var result = LongOperationDlg.StartOperation(
				delegate(IWorker worker) {
					PrintDocuments(worker, documents);
				},
				"Печать файлов...",
				documents.Count()
			);
			if (result == LongOperationResult.Canceled)
				return;
		}	

		public static void PrintDocuments(IWorker worker, IList<IPrintableOdtDocument> docs)
		{
			using (FileWorker fileWorker = new FileWorker())
			{
				int step = 0;
				foreach (var document in docs)
				{
					worker.ReportProgress(step, document.Name);
					var template = document.GetTemplate();
					int copies = document.CopiesToPrint;
					if (template != null)
						fileWorker.OpenInOffice(template, true, FileEditMode.Document, copies, PrintSettings);
					if (worker.IsCancelled)
						return;
					step++;
				}
			}
		}
	}

	public interface IPrintableOdtDocument : IPrintableDocument
	{
		IDocTemplate GetTemplate();
	}
}