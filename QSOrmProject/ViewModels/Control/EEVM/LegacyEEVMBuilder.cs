using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.ViewModels.Dialog;
using QS.ViewModels.Resolve;

namespace QS.ViewModels.Control.EEVM
{
	public class LegacyEEVMBuilder<TEntity> : CommonEEVMBuilder<TEntity>
		where TEntity : class, IDomainObject
	{
		readonly ILegacyEEVMBuilderParameters legacyParameters;

		public LegacyEEVMBuilder(ILegacyEEVMBuilderParameters legacyParameters, IPropertyBinder<TEntity> propertyBinder = null) : base(legacyParameters, propertyBinder)
		{
			this.legacyParameters = legacyParameters;
		}

		#region Fluent Config

		public LegacyEEVMBuilder<TEntity> UseOrmReferenceJournal()
		{
			EntitySelector = new OrmReferenceSelector(legacyParameters.GetDialogTab, parameters.UnitOfWork, typeof(TEntity));
			return this;
		}

		public LegacyEEVMBuilder<TEntity> UseOrmReferenceJournal(QueryOver itemsQuery)
		{
			EntitySelector = new OrmReferenceSelector(legacyParameters.GetDialogTab, parameters.UnitOfWork, itemsQuery);
			return this;
		}

		public LegacyEEVMBuilder<TEntity> UseOrmReferenceJournal(ICriteria itemsCriteria)
		{
			EntitySelector = new OrmReferenceSelector(legacyParameters.GetDialogTab, parameters.UnitOfWork, itemsCriteria);
			return this;
		}

		public LegacyEEVMBuilder<TEntity> UseOrmReferenceJournalAndAutocompleter()
		{
			EntitySelector = new OrmReferenceSelector(legacyParameters.GetDialogTab, parameters.UnitOfWork, typeof(TEntity));
			EntityAutocompleteSelector = new OrmReferenceAutocompleteSelector<TEntity>(parameters.UnitOfWork);
			return this;
		}

		public LegacyEEVMBuilder<TEntity> UseOrmReferenceJournalAndAutocompleter(QueryOver itemsQuery)
		{
			EntitySelector = new OrmReferenceSelector(legacyParameters.GetDialogTab, parameters.UnitOfWork, itemsQuery);
			EntityAutocompleteSelector = new OrmReferenceAutocompleteSelector<TEntity>(parameters.UnitOfWork, itemsQuery);
			return this;
		}

		public LegacyEEVMBuilder<TEntity> UseOrmReferenceJournalAndAutocompleter(ICriteria itemsCriteria)
		{
			EntitySelector = new OrmReferenceSelector(legacyParameters.GetDialogTab, parameters.UnitOfWork, itemsCriteria);
			EntityAutocompleteSelector = new OrmReferenceAutocompleteSelector<TEntity>(parameters.UnitOfWork, itemsCriteria);
			return this;
		}

		public LegacyEEVMBuilder<TEntity> UseTdiEntityDialog()
		{
			EntityDlgOpener = new OrmObjectDialogOpener<TEntity>(legacyParameters.GetDialogTab);
			return this;
		}

		#endregion


		#region Перехват вызовов для работы без диалогов ParentViewModel

		public new virtual CommonEEVMBuilder<TEntity> UseViewModelJournal<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase
		{
			if(parameters.DialogViewModel == null)
				EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel>(legacyParameters.GetDialogTab, parameters.UnitOfWork, parameters.NavigationManager);
			else
				base.UseViewModelJournal<TJournalViewModel>();
			return this;
		}

		public new virtual CommonEEVMBuilder<TEntity> UseViewModelJournalAndAutocompleter<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase
		{
			if(parameters.DialogViewModel == null) {
				EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel>(legacyParameters.GetDialogTab, parameters.UnitOfWork, parameters.NavigationManager);
				EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel>(parameters.UnitOfWork, parameters.AutofacScope);
			}
			else
				base.UseViewModelJournalAndAutocompleter<TJournalViewModel>();
			return this;
		}

		public new virtual CommonEEVMBuilder<TEntity> UseViewModelDialog<TEntityViewModel>()
			where TEntityViewModel : DialogViewModelBase
		{
			if(legacyParameters.GetDialogTab != null)
				EntityDlgOpener = new EntityViewModelOpener<TEntityViewModel>(parameters.NavigationManager, legacyParameters.GetDialogTab);
			else
				EntityDlgOpener = new EntityViewModelOpener<TEntityViewModel>(parameters.NavigationManager, parameters.DialogViewModel);
			return this;
		}

		public new virtual CommonEEVMBuilder<TEntity> MakeByType()
		{
			if(parameters.DialogViewModel == null) {
				if(parameters.AutofacScope == null)
					throw new NullReferenceException($"{nameof(parameters.AutofacScope)} не задан для билдера. Без него использование {nameof(MakeByType)} невозможно.");
				var resolver = parameters.AutofacScope.Resolve<IViewModelResolver>();

				var journalViewModelType = resolver.GetTypeOfViewModel(typeof(TEntity), TypeOfViewModel.Journal);
				if(journalViewModelType != null) {
					var entitySelectorType = typeof(JournalViewModelSelector<,>).MakeGenericType(typeof(TEntity), journalViewModelType);
					EntitySelector = (IEntitySelector)Activator.CreateInstance(entitySelectorType, legacyParameters.GetDialogTab, parameters.UnitOfWork, parameters.NavigationManager);

					var entityAutocompleteSelectorType = typeof(JournalViewModelAutocompleteSelector<,>).MakeGenericType(typeof(TEntity), journalViewModelType);
					EntityAutocompleteSelector = (IEntityAutocompleteSelector<TEntity>)Activator.CreateInstance(entityAutocompleteSelectorType, parameters.UnitOfWork, parameters.AutofacScope);
				}

				var dialogViewModelType = resolver.GetTypeOfViewModel(typeof(TEntity), TypeOfViewModel.EditDialog);
				if(dialogViewModelType != null) {
					var entityDlgOpenerType = typeof(EntityViewModelOpener<>).MakeGenericType(dialogViewModelType);
					EntityDlgOpener = (IEntityDlgOpener)Activator.CreateInstance(entityDlgOpenerType, parameters.NavigationManager, legacyParameters.GetDialogTab);
				}
			}
			else
				base.MakeByType();
			return this;
		}

		#endregion
	}
}
