using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;

namespace QS.ViewModels
{
	public abstract class UoWTabViewModelBase : TabViewModelBase, ISingleUoWDialog, IDisposable
	{
		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;

		protected UoWTabViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation) : base(interactiveService, navigation)
		{
			this.UnitOfWorkFactory = unitOfWorkFactory;
		}

		private IUnitOfWork unitOfWork;

		public virtual IUnitOfWork UoW { 
			get {
				if(unitOfWork == null)
					unitOfWork = UnitOfWorkFactory.Create(Title);

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
