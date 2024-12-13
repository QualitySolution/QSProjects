using System;
using QS.DomainModel.UoW;

namespace QS.Report.ViewModels {
	public abstract class ReportParametersUowViewModelBase : ReportParametersViewModelBase {
		protected ReportParametersUowViewModelBase(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWork unitOfWork
			) : base(rdlViewerViewModel) {
			this.UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}
		public virtual IUnitOfWork UoW { get; private set; }
	}
}
