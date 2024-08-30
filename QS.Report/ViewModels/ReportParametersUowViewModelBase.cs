using System;
using QS.DomainModel.UoW;

namespace QS.Report.ViewModels {
	public abstract class ReportParametersUowViewModelBase : ReportParametersViewModelBase, IDisposable {
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly UnitOfWorkProvider unitOfWorkProvider;

		protected ReportParametersUowViewModelBase(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IReportInfoFactory reportInfoFactory,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(rdlViewerViewModel, reportInfoFactory) {
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		
		private IUnitOfWork unitOfWork;

		public virtual IUnitOfWork UoW {
			get {
				if(unitOfWork == null) {
					unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
					if(unitOfWorkProvider != null)
						unitOfWorkProvider.UoW = unitOfWork;
				}

				return unitOfWork;
			}
		}
		
		public virtual void Dispose()
		{
			UoW?.Dispose();
		}
	}
}
