using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Report.ViewModels
{
	public abstract class ReportParametersViewModelBase : ViewModelBase
	{
		private readonly RdlViewerViewModel rdlViewerViewModel;
		private readonly IReportInfoFactory reportInfoFactory;

		protected ReportParametersViewModelBase(RdlViewerViewModel rdlViewerViewModel, IReportInfoFactory reportInfoFactory)
		{
			this.rdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
			this.reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
		}

		#region Свойства
		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private string identifier;

		public virtual string Identifier {
			get => identifier;
			set => SetField(ref identifier, value);
		}
		#endregion

		#region Параметры отчета
		protected abstract Dictionary<string, object> Parameters { get; }

		public virtual ReportInfo ReportInfo => reportInfoFactory.Create(Identifier, Title, Parameters);

		#endregion

		#region Действия View
		public void LoadReport()
		{
			rdlViewerViewModel.LoadReport();
		}
		#endregion
	}
}
