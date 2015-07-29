using System;
using QSProjectsLib;

namespace QSReport
{
	public partial class ReportViewDlg : QSTDI.TdiTabBase
	{
		private ReportInfo reportInfo;

		public ReportViewDlg (ReportInfo info)
		{
			reportInfo = info;

			this.Build ();

			TabName = reportInfo.Title;

			reportviewer1.LoadReport (reportInfo.GetReportUri (), reportInfo.GetParametersString (), QSMain.ConnectionString);
		}
	}
}

