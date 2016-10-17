using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using QSProjectsLib;

namespace QSDocTemplates
{
	public static class TemplatePrinter
	{		
		
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
					if (HasTemplate(template))
						fileWorker.OpenInOffice(template, true, FileEditMode.Document, true);
					if (worker.IsCancelled)
						return;
					step++;
				}
			}
		}

		private static bool HasTemplate (IDocTemplate template)
		{
			return template != null;
		}
	}		

	public interface ITemplatePrntDoc
	{
		IDocTemplate GetTemplate();
		string Name { get;}
	}

}