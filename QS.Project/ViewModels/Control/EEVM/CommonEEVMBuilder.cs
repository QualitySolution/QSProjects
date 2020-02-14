using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public class CommonEEVMBuilder<TEntity>
		where TEntity: class, IDomainObject
	{
		#region Обазательные параметры
		protected IEEVMBuilderParameters parameters;
		protected IPropertyBinder<TEntity> PropertyBinder;
		#endregion

		#region Опциональные компоненты
		protected IEntitySelector EntitySelector;
		protected IEntityDlgOpener EntityDlgOpener;
		protected IEntityAutocompleteSelector<TEntity> EntityAutocompleteSelector;
		#endregion

		public CommonEEVMBuilder(IEEVMBuilderParameters buildParameters, IPropertyBinder<TEntity> propertyBinder = null)
		{
			this.parameters = buildParameters;
			this.PropertyBinder = propertyBinder;
		}

		#region Fluent Config

		public virtual CommonEEVMBuilder<TEntity> UseViewModelJournal<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase
		{
			EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel>(parameters.DialogViewModel, parameters.UnitOfWork, parameters.NavigationManager);
			return this;
		}

		public virtual CommonEEVMBuilder<TEntity> UseViewModelJournalAndAutocompleter<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase
		{
			EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel>(parameters.DialogViewModel, parameters.UnitOfWork, parameters.NavigationManager);
			EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel>(parameters.UnitOfWork, parameters.AutofacScope);
			return this;
		}

		public virtual CommonEEVMBuilder<TEntity> UseViewModelDialog<TEntityViewModel>()
			where TEntityViewModel : DialogViewModelBase
		{
			EntityDlgOpener = new EntityViewModelOpener<TEntityViewModel>(parameters.NavigationManager, parameters.DialogViewModel);
			return this;
		}

		public virtual EntityEntryViewModel<TEntity> Finish()
		{
			return new EntityEntryViewModel<TEntity>(PropertyBinder, EntitySelector, EntityDlgOpener, EntityAutocompleteSelector);
		}

		#endregion
	}
}