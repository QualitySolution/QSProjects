using System.Collections.Generic;
using System.Linq;
using fyiReporting.RDL;
using fyiReporting.RdlGtkViewer;
using Gtk;
using NLog;
using QS.Print;
using QS.Report;
using QSProjectsLib;

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
				var rdlDoc = document as IPrintableRDLDocument;
				rdlDoc.Parameters = new Dictionary<object, object> {
					{ "IsBatchPrint", true }
				};
				Report report = GetReportFromFileOrMemory(rdlDoc.GetReportInfo());
				report.RunGetData(rdlDoc.GetReportInfo().Parameters);
				reportPages.Add(report.BuildPages());
			}
		}

		private Report GetReportFromFileOrMemory(ReportInfo reportInfo)
		{
			RDLParser rdlp;
			Report r;
			string source;
			if(reportInfo.Source == null)
				source = System.IO.File.ReadAllText(reportInfo.GetPath());
			else
				source = reportInfo.Source;

			rdlp = new RDLParser(source) {
				Folder = System.IO.Path.GetDirectoryName (reportInfo.GetPath()),
				OverwriteConnectionString = reportInfo.ConnectionString,
				OverwriteInSubreport = true
			};

			r = rdlp.Parse();

			return r;
		}

		public void DrawPage(object o, DrawPageArgs args) {
			using(var g = args.Context.CairoContext) {
				CalculateDocPage(args.PageNr, out var reportNumber, out var pageNumber);

				using(var render = new RenderCairo(g)) {
					render.RunPage(reportPages[reportNumber][pageNumber]);
				}
			}
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

