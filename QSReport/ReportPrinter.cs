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
			string source = System.IO.File.ReadAllText(ReportInfo.GetPath());

			rdlp = new RDLParser(source);
			rdlp.Folder = System.IO.Path.GetDirectoryName(ReportInfo.GetPath());
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
