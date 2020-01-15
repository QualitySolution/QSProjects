using System;
using System.ComponentModel;
using System.Linq.Expressions;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public class CommonEEVMBuilder<TBindedEntity, TEntity>
		where TBindedEntity: class, INotifyPropertyChanged
		where TEntity: class, IDomainObject
	{
		#region Обазательные параметры
		protected CommonEEVMBuilderFactory<TBindedEntity> factory;
		protected IPropertyBinder<TEntity> PropertyBinder;
		#endregion

		#region Опциональные компоненты
		protected IEntitySelector EntitySelector;
		protected IEntityDlgOpener EntityDlgOpener;
		protected IEntityAutocompleteSelector<TEntity> EntityAutocompleteSelector;
		#endregion

		public CommonEEVMBuilder(CommonEEVMBuilderFactory<TBindedEntity> builderFactory, Expression<Func<TBindedEntity, TEntity>> bindedProperty)
		{
			this.factory = builderFactory;
			PropertyBinder = new PropertyBinder<TBindedEntity, TEntity>(factory.BindedEntity, bindedProperty);
		}

		#region Fluent Config

		public CommonEEVMBuilder<TBindedEntity, TEntity> UseViewModelJournal<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase
		{
			EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel>(factory.DialogViewModel, factory.UnitOfWork, factory.NavigationManager);
			return this;
		}

		public CommonEEVMBuilder<TBindedEntity, TEntity> UseViewModelJournalAndAutocompleter<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase
		{
			EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel>(factory.DialogViewModel, factory.UnitOfWork, factory.NavigationManager);
			EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel>(factory.UnitOfWork, factory.AutofacScope);
			return this;
		}

		public CommonEEVMBuilder<TBindedEntity, TEntity> UseViewModelDialog<TEntityViewModel>()
			where TEntityViewModel : DialogViewModelBase
		{
			EntityDlgOpener = new EntityViewModelOpener<TEntityViewModel>(factory.NavigationManager, factory.DialogViewModel);
			return this;
		}

		public EntityEntryViewModel<TEntity> Finish()
		{
			return new EntityEntryViewModel<TEntity>(PropertyBinder, EntitySelector, EntityDlgOpener, EntityAutocompleteSelector);
		}

		#endregion
	}
}