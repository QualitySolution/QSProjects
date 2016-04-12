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

		public override bool CompareHashName(string hashName)
		{
			return GenerateHashName(reportInfo) == hashName;
		}

		public static string GenerateHashName(ReportInfo reportInfo)
		{
			string parameters = "_";
			foreach(var pair in reportInfo.Parameters)
			{
				parameters += String.Format("{0}={1}", pair.Key, pair.Value);
			}

			if (parameters == "_")
				parameters = String.Empty;

			return String.Format("Report_{0}{1}", reportInfo.Identifier, parameters);
		}

		public ReportViewDlg (ReportInfo info)
		{
			reportInfo = info;

			this.Build ();

			TabName = reportInfo.Title;

			panelParameters.Visible = false;

			reportviewer1.LoadReport (reportInfo.GetReportUri (), reportInfo.GetParametersString (), QSMain.ConnectionString, true);
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
				QSMain.ConnectionString,
				true
			);
		}
	}
}

