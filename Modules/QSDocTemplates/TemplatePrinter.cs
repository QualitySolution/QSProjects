using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using QSProjectsLib;

namespace QSDocTemplates
{
	public static class TemplatePrinter
	{
		public static PrintSettings PrintSettings { get; set; }

		public static void Print(ITemplatePrntDoc document)
		{
			PrintAll(new ITemplatePrntDoc[]{ document });
		}

		public static void PrintAll(IList<ITemplatePrntDoc> documents)
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

		public static void PrintDocuments(IWorker worker, IList<ITemplatePrntDoc> docs)
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

	public interface ITemplatePrntDoc
	{
		IDocTemplate GetTemplate();
		string Name { get; }
		int CopiesToPrint { get; set; }
	}
}