using System;
using Gtk;
using QS;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.Report.Repository;

namespace QSReport
{
	public partial class ReportViewDlg : QS.Dialog.Gtk.TdiTabBase, IDisposable
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private ReportInfo reportInfo;
		public IParametersWidget ParametersWidget { get; }

		public event EventHandler ReportPrinted;

		public override bool CompareHashName(string hashName)
		{
			if (reportInfo != null)
				return GenerateHashName(reportInfo) == hashName;
			else if (ParametersWidget != null)
				return GenerateHashName(ParametersWidget) == hashName;
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

		public ReportViewDlg (IParametersWidget widget)
		{
			this.Build ();

			ParametersWidget = widget;

			TabName = ParametersWidget.Title;
			reportviewer1.DefaultExportFileName = ParametersWidget.Title;

			panelParameters.Visible = true;

			panelParameters.Panel = ParametersWidget as Widget;

			ParametersWidget.LoadReport += ParametersWidget_LoadReport;
		}

		void LoadReport(ReportInfo info)
		{
			logger.Debug (String.Format ("Report Parameters[{0}]", info.GetParametersString ()));

			MultiplePrintOperation multiplePrintOperation = null;

			if (info.PrintType == ReportInfo.PrintingType.MultiplePrinters)
			{
				var commonService = ServicesConfig.CommonServices;
				var userPrintSettingsRepository = new UserPrintingRepository();
				multiplePrintOperation = new MultiplePrintOperation(ServicesConfig.UnitOfWorkFactory, commonService, userPrintSettingsRepository);
			}

			if(info.Source != null)
			{
				reportviewer1.LoadReport(info.Source, info.GetParametersString(), info.ConnectionString, true, info.RestrictedOutputPresentationTypes);
			}
			else
			{
				if (multiplePrintOperation == null)
				{
					reportviewer1.LoadReport(info.GetReportUri(), info.GetParametersString(), info.ConnectionString, true, info.RestrictedOutputPresentationTypes);
				}
				else
				{
					reportviewer1.LoadReport(info.GetReportUri(), info.GetParametersString(), info.ConnectionString, true, info.RestrictedOutputPresentationTypes, multiplePrintOperation.Run);
				}
			}
			if(!string.IsNullOrWhiteSpace(info.Title))
			{
				reportviewer1.DefaultExportFileName = info.Title;
			}
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
			ParametersWidget?.Destroy();
			base.Destroy();
		}

        public override void Dispose()
        {
			reportviewer1?.Dispose();
			reportviewer1 = null;
			if(ParametersWidget != null) {
				ParametersWidget.LoadReport -= ParametersWidget_LoadReport;
			}
			base.Dispose();
        }
    }
}

