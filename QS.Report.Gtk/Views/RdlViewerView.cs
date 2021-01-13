using System;
using Autofac;
using QS.Report.ViewModels;
using QS.Views.Resolve;

namespace QS.Report.Views
{
	public partial class RdlViewerView : Gtk.Bin
	{
		RdlViewerViewModel ViewModel;

		public RdlViewerView(RdlViewerViewModel viewModel)
		{
			this.Build();
			ViewModel = viewModel;

			reportviewer1.DefaultExportFileName = ViewModel.ReportInfo?.Title;

			if(ViewModel.ReportParametersViewModel != null) {
				var resolver = ViewModel.AutofacScope.Resolve<IGtkViewResolver>();
				var widget = resolver.Resolve(ViewModel.ReportParametersViewModel);
				panelParameters.Panel = widget;
				ViewModel.LoadReport = LoadReport;
			}
			else {
				panelParameters.Visible = false;
				LoadReport();
			}
		}

		public void LoadReport()
		{
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
