using System;
using QS.Navigation;
using QS.ViewModels;

namespace QS.Report.ViewModels
{
	public class RdlViewerViewModel : DialogViewModelBase
	{
		public readonly ReportInfo ReportInfo;

		public event EventHandler ReportPrinted;

		public RdlViewerViewModel(ReportInfo reportInfo, INavigationManager navigation) : base(navigation)
		{
			this.ReportInfo = reportInfo;
			Title = reportInfo.Title;
		}

		public void RiseReportPrinted()
		{
			ReportPrinted?.Invoke(this, EventArgs.Empty);
		}
	}
}
