using System;
using QS.DomainModel.UoW;
using QS.Navigation;

namespace QS.ViewModels.Dialog
{
	public abstract class UowDialogViewModelBase : DialogViewModelBase, IDisposable
	{
		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;

		protected UowDialogViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation) : base(navigation)
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

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoW.HasChanges; }
			set { manualChange = value; }
		}

		public virtual void Save()
		{
			if(UoW.RootObject != null)
				UoW.Save();
			else
				UoW.Commit();
		}

		public virtual void SaveAndClose()
		{
			Save();
			Close(false);
		}

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}
	}
}
