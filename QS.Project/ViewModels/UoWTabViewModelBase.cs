using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Tdi;

namespace QS.ViewModels
{
	public abstract class UoWTabViewModelBase : TabViewModelBase, ISingleUoWDialog, IDisposable
	{
		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;

		protected UoWTabViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService) : base(interactiveService)
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

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}
	}
}
