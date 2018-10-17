using System;
using QS.Report;

namespace QSReport
{
	public interface IParametersWidget
	{
		event EventHandler<LoadReportEventArgs> LoadReport;

		string Title { get;}
	}

	public class LoadReportEventArgs : EventArgs
	{
		public ReportInfo Info {get; set;}
		public bool HideParameters {get; set;}

		public LoadReportEventArgs(ReportInfo report, bool hideparameters = false)
		{
			Info = report;
			HideParameters = hideparameters;
		}
	}
}

