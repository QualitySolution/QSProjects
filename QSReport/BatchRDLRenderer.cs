using System;
using fyiReporting.RDL;
using QSProjectsLib;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using fyiReporting.RdlGtkViewer;
using NLog;

namespace QSReport
{
	public class BatchRDLRenderer
	{		
		IList<Pages> reportPages;
		IList<IPrintableDocument> documents;
		private Logger log = LogManager.GetCurrentClassLogger ();

		public int PageCount
		{
			get
			{
				return reportPages.Sum(pages => pages.PageCount);
			}
		}

		public BatchRDLRenderer(IEnumerable<IPrintableDocument> documents)
		{							
			this.documents = documents.ToList();
			reportPages = new List<Pages>();
		}

		public void PrepareDocuments(IWorker worker)
		{
			int step = 0;
			foreach (var document in documents)
			{
				worker.ReportProgress(step, document.Name);
				Prepare(document);
				if (worker.IsCancelled)
					return;
				step++;
			}
		}

		protected void Prepare(IPrintableDocument document)
		{
			if (document.PrintType == PrinterType.RDL)
			{
				Report report = GetReportFromFile(document.GetReportInfo());
				report.RunGetData(document.GetReportInfo().Parameters);
				reportPages.Add(report.BuildPages());
			}
		}

		private Report GetReportFromFile(ReportInfo reportInfo)
		{
			RDLParser rdlp;
			Report r;
			string source = System.IO.File.ReadAllText(reportInfo.GetPath());

			rdlp = new RDLParser(source);
			rdlp.Folder = System.IO.Path.GetDirectoryName (reportInfo.GetPath());
			rdlp.OverwriteConnectionString = reportInfo.ConnectionString;
			rdlp.OverwriteInSubreport = true;

			r = rdlp.Parse();

			return r;
		}

		public void DrawPage(object o, DrawPageArgs args){
			var g = args.Context.CairoContext;
			int pageNumber, reportNumber;
			CalculateDocPage(args.PageNr, out reportNumber, out pageNumber);

			RenderCairo render = new RenderCairo(g);

			render.RunPage(reportPages[reportNumber][pageNumber]);
		}

		public void RequestPageSetup (object o, RequestPageSetupArgs args)
		{
			int docnum, page;
			CalculateDocPage(args.PageNr, out docnum, out page);
			args.Setup.Orientation = documents[docnum].Orientation == DocumentOrientation.Landscape 
				? PageOrientation.Landscape : PageOrientation.Portrait;
		}

		void CalculateDocPage(int printPage, out int docNum, out int pageNum)
		{
			pageNum = printPage;
			docNum = 0;
			while (pageNum >= reportPages[docNum].PageCount)
			{
				pageNum -= reportPages[docNum].PageCount;
				docNum++;
			}
		}
	}
}

