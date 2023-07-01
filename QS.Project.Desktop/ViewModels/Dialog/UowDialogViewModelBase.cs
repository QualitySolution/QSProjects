using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;

namespace QS.ViewModels.Dialog
{
	public abstract class UowDialogViewModelBase : DialogViewModelBase, IDisposable
	{
		protected UnitOfWorkProvider UnitOfWorkProvider { get; }
		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;

		protected UowDialogViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			string UoWTitle = null,
			UnitOfWorkProvider unitOfWorkProvider = null) : base(navigation)
		{
			UnitOfWorkProvider = unitOfWorkProvider;
			this.UnitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.validator = validator;
			uoWTitle = UoWTitle;
		}

		private IUnitOfWork unitOfWork;

		public virtual IUnitOfWork UoW {
			get {
				if(unitOfWork == null) {
					unitOfWork = UnitOfWorkFactory.CreateWithoutRoot(uoWTitle);
					if(UnitOfWorkProvider != null)
						UnitOfWorkProvider.UoW = unitOfWork;
				}

				return unitOfWork;
			}
		}

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoW.HasChanges; }
			set { manualChange = value; }
		}

		public virtual bool Save()
		{
			if(!Validate())
				return false;

			if(UoW.RootObject != null)
				UoW.Save();
			else
				UoW.Commit();
			return true;
		}

		public bool SaveAndClose()
		{
			if (Save()) {
				Close(false, CloseSource.Save);
				return true;
			}
			return false;
		}

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}

		#region Валидация

		private readonly IValidator validator;
		private readonly string uoWTitle;
		public readonly List<ValidationRequest> Validations = new List<ValidationRequest>();

		protected virtual bool Validate()
		{
			if(validator == null || !Validations.Any())
				return true;

			return validator.Validate(Validations);
		}

		#endregion
	}
}
