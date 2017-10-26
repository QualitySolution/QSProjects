using System;
using fyiReporting.RDL;
using fyiReporting.RdlGtkViewer;
using Gtk;

namespace QSReport
{
	public class ReportPrinter
	{
		ReportInfo ReportInfo;
		Report Report;
		Pages Pages;

		public int PageCount{ get { return Pages.PageCount; }}

		public ReportPrinter(ReportInfo info)
		{
			ReportInfo = info;
		}

		public void PrepareReport()
		{
			RDLParser rdlp;
			var reportPath = ReportInfo.GetPath();
			string source = ReportInfo.Source ?? System.IO.File.ReadAllText(reportPath);

			rdlp = new RDLParser(source);
			if(reportPath != null)
				rdlp.Folder = System.IO.Path.GetDirectoryName(reportPath);
			rdlp.OverwriteConnectionString = ReportInfo.ConnectionString;
			rdlp.OverwriteInSubreport = true;

			Report = rdlp.Parse();

			Report.RunGetData(ReportInfo.Parameters);
			Pages = Report.BuildPages();
		}

		public void DrawPage(object o, DrawPageArgs args)
		{
			var g = args.Context.CairoContext;
			RenderCairo render = new RenderCairo(g);

			render.RunPage(Pages[args.PageNr]);
		}
	}
}
