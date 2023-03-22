using System;
using Autofac;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Report.ViewModels
{
	public class RdlViewerViewModel : DialogViewModelBase, IDisposable
	{
		public ReportParametersViewModelBase ReportParametersViewModel { get; private set; }
		public ILifetimeScope AutofacScope { get; }

		private readonly ReportInfo reportInfo;
		public ReportInfo ReportInfo => ReportParametersViewModel?.ReportInfo ?? reportInfo;

		public event EventHandler ReportPrinted;

		public Action LoadReport;

		public RdlViewerViewModel(ReportInfo reportInfo, INavigationManager navigation) : base(navigation)
		{
			this.reportInfo = reportInfo;
			Title = reportInfo.Title;
		}

		public RdlViewerViewModel(Type reportParametersViewModelType, ILifetimeScope autofacScope, INavigationManager navigation) : base(navigation)
		{
			AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			ReportParametersViewModel = (ReportParametersViewModelBase)autofacScope.Resolve(reportParametersViewModelType, 
				new TypedParameter(typeof(RdlViewerViewModel), this));
			ReportParametersViewModel.PropertyChanged += ReportParametersViewModel_PropertyChanged;
			Title = ReportParametersViewModel.Title;
		}

		void ReportParametersViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ReportParametersViewModel.Title))
				Title = ReportParametersViewModel.Title;
		}

		public void RiseReportPrinted()
		{
			ReportPrinted?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose() {
			if(ReportParametersViewModel is IDisposable disposable)
				disposable.Dispose();
		}
	}
}
