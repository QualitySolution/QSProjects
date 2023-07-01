using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Report.ViewModels
{
	public abstract class ReportParametersViewModelBase : ViewModelBase
	{
		private readonly RdlViewerViewModel rdlViewerViewModel;

		protected ReportParametersViewModelBase(RdlViewerViewModel rdlViewerViewModel)
		{
			this.rdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
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

		public virtual ReportInfo ReportInfo => new ReportInfo {
			Identifier = Identifier,
			Title = Title,
			Parameters = Parameters
		};
		#endregion

		#region Действия View
		public void LoadReport()
		{
			rdlViewerViewModel.LoadReport();
		}
		#endregion
	}
}
