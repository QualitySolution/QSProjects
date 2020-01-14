using System;
using QS.Report.ViewModels;

namespace QS.Report.Views
{
	public partial class RdlViewerView : Gtk.Bin
	{
		RdlViewerViewModel ViewModel;

		public RdlViewerView(RdlViewerViewModel model)
		{
			this.Build();
			ViewModel = model;

			reportviewer1.DefaultExportFileName = ViewModel.ReportInfo.Title;

			//FIXME Реализовать режим работы с панелью. Видимо нужно делать отдельно новый тип панелей на ViewModel.
			panelParameters.Visible = false;

			if(ViewModel.ReportInfo.Source != null)
				reportviewer1.LoadReport(ViewModel.ReportInfo.Source, ViewModel.ReportInfo.GetParametersString(), ViewModel.ReportInfo.ConnectionString, true);
			else
				reportviewer1.LoadReport(ViewModel.ReportInfo.GetReportUri(), ViewModel.ReportInfo.GetParametersString(), ViewModel.ReportInfo.ConnectionString, true);
		}

		protected void OnReportviewer1ReportPrinted(object sender, EventArgs e)
		{
			ViewModel.RiseReportPrinted();
		}
	}
}
