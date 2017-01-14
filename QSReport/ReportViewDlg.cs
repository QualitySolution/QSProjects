﻿using System;
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
			reportviewer1.LoadReport (info.GetReportUri (), info.GetParametersString (), info.ConnectionString, true);
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
	}
}

