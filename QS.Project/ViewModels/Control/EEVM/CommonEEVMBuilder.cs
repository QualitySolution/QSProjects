using System;
using Autofac;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.ViewModels.Dialog;
using QS.ViewModels.Resolve;

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
		protected IEntityAdapter<TEntity> EntityAdapter;
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

		public virtual CommonEEVMBuilder<TEntity> MakeByType()
		{
			if (parameters.AutofacScope == null)
				throw new NullReferenceException($"{nameof(parameters.AutofacScope)} не задан для билдера. Без него использование {nameof(MakeByType)} невозможно.");
			var resolver = parameters.AutofacScope.Resolve<IViewModelResolver>();

			var journalViewModelType = resolver.GetTypeOfViewModel(typeof(TEntity), TypeOfViewModel.Journal);
			if(journalViewModelType != null) {
				var entitySelectorType = typeof(JournalViewModelSelector<,>).MakeGenericType(typeof(TEntity), journalViewModelType);
				EntitySelector = (IEntitySelector)Activator.CreateInstance(entitySelectorType, parameters.DialogViewModel, parameters.UnitOfWork, parameters.NavigationManager);

				var entityAutocompleteSelectorType = typeof(JournalViewModelAutocompleteSelector<,>).MakeGenericType(typeof(TEntity), journalViewModelType);
				EntityAutocompleteSelector = (IEntityAutocompleteSelector<TEntity>) Activator.CreateInstance(entityAutocompleteSelectorType, parameters.UnitOfWork, parameters.AutofacScope);
			}
			
			var dialogViewModelType = resolver.GetTypeOfViewModel(typeof(TEntity), TypeOfViewModel.EditDialog);
			if(dialogViewModelType != null) {
				var entityDlgOpenerType = typeof(EntityViewModelOpener<>).MakeGenericType(dialogViewModelType);
				EntityDlgOpener = (IEntityDlgOpener) Activator.CreateInstance(entityDlgOpenerType, parameters.NavigationManager, parameters.DialogViewModel);
			}
			return this;
		}

		public virtual CommonEEVMBuilder<TEntity> UseAdapter(IEntityAdapter<TEntity> adapter)
		{
			EntityAdapter = adapter;
			return this;
		}

		public virtual CommonEEVMBuilder<TEntity> UseFuncAdapter(Func<object, TEntity> getEntityByNode)
		{
			EntityAdapter = new FuncEntityAdapter<TEntity>(getEntityByNode);
			return this;
		}

		public virtual EntityEntryViewModel<TEntity> Finish()
		{
			var entityAdapter = EntityAdapter ?? new UowEntityAdapter<TEntity>(parameters.UnitOfWork);
			return new EntityEntryViewModel<TEntity>(PropertyBinder, EntitySelector, EntityDlgOpener, EntityAutocompleteSelector, entityAdapter);
		}

		#endregion
	}
}