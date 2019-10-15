using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.ViewModels
{
	public class UoWTabViewModelBase : TabViewModelBase, ISingleUoWDialog, IDisposable
	{
		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;

		public UoWTabViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService) : base(interactiveService)
		{
			this.UnitOfWorkFactory = unitOfWorkFactory;
		}

		private IUnitOfWork unitOfWork;

		public virtual IUnitOfWork UoW { 
			get {
				if(unitOfWork == null)
					unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

				return unitOfWork;
			}
			set => unitOfWork = value;
		 }

		public virtual void Dispose()
		{
			if(UoW != null) {
				UoW.Dispose();
			}
		}
	}
}
