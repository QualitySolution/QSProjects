using System;
using Gtk;
using QS.Report;

namespace QSReport
{
	public partial class ReportViewDlg : QS.Dialog.Gtk.TdiTabBase, IDisposable
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private ReportInfo reportInfo;
		private IParametersWidget parametersWidget;

		public event EventHandler ReportPrinted;

		public override bool CompareHashName(string hashName)
		{
			if (reportInfo != null)
				return GenerateHashName(reportInfo) == hashName;
			else if (parametersWidget != null)
				return GenerateHashName(parametersWidget) == hashName;
			else
				return false;
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

		public static string GenerateHashName(IParametersWidget parametersWidget)
		{
			return String.Format("Report_{0}", parametersWidget.GetType().Name);
		}

		public static new string GenerateHashName<T>() where T : IParametersWidget => String.Format("Report_{0}", typeof(T).Name);

		public ReportViewDlg (ReportInfo info)
		{
			reportInfo = info;

			this.Build ();

			TabName = reportInfo.Title;
			reportviewer1.DefaultExportFileName = reportInfo.Title;

			panelParameters.Visible = false;

			LoadReport(reportInfo);
		}

		void LoadReport(ReportInfo info)
		{
			logger.Debug (String.Format ("Report Parameters[{0}]", info.GetParametersString ()));
			if(info.Source != null)
				reportviewer1.LoadReport(info.Source, info.GetParametersString(), info.ConnectionString, true, info.RestrictedOutputPresentationTypes);
			else
				reportviewer1.LoadReport(info.GetReportUri(), info.GetParametersString(), info.ConnectionString, true, info.RestrictedOutputPresentationTypes);
		}

		public ReportViewDlg (IParametersWidget widget)
		{
			this.Build ();

			parametersWidget = widget;

			TabName = parametersWidget.Title;
			reportviewer1.DefaultExportFileName = parametersWidget.Title;

			panelParameters.Visible = true;

			panelParameters.Panel = parametersWidget as Widget;

			parametersWidget.LoadReport += ParametersWidget_LoadReport;
		}

		void ParametersWidget_LoadReport (object sender, LoadReportEventArgs e)
		{
			LoadReport(e.Info);
		}

		protected void OnReportviewer1ReportPrinted(object sender, EventArgs e)
		{
			ReportPrinted?.Invoke(this, EventArgs.Empty);
		}

		public override void Destroy()
		{
			parametersWidget?.Destroy();
			base.Destroy();
		}

        public override void Dispose()
        {
			reportviewer1?.Dispose();
			reportviewer1 = null;
			if(parametersWidget != null) {
				parametersWidget.LoadReport -= ParametersWidget_LoadReport;
			}
			base.Dispose();
        }
    }
}

