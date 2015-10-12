using System;
using QSProjectsLib;
using Gtk;

namespace QSReport
{
	public partial class ReportViewDlg : QSTDI.TdiTabBase
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private ReportInfo reportInfo;
		private IParametersWidget parametersWidget;

		public ReportViewDlg (ReportInfo info)
		{
			reportInfo = info;

			this.Build ();

			TabName = reportInfo.Title;

			panelParameters.Visible = false;

			reportviewer1.LoadReport (reportInfo.GetReportUri (), reportInfo.GetParametersString (), QSMain.ConnectionString);
		}

		public ReportViewDlg (IParametersWidget widget)
		{
			this.Build ();

			parametersWidget = widget;

			TabName = parametersWidget.Title;

			panelParameters.Visible = true;

			panelParameters.Panel = parametersWidget as Widget;

			parametersWidget.LoadReport += ParametersWidget_LoadReport;
		}

		void ParametersWidget_LoadReport (object sender, LoadReportEventArgs e)
		{
			logger.Debug (String.Format ("Report Parameters[{0}]", e.Info.GetParametersString ()));
			reportviewer1.LoadReport (e.Info.GetReportUri (),
				e.Info.GetParametersString (),
				QSMain.ConnectionString);
		}
	}
}

