using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Dialog;
using QS.ViewModels.Resolve;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace QS.ViewModels.Control.EEVM {
	/// <summary>
	/// Билдер ViewModel для EntityEntry
	/// Можно зарегистрировать в контейнере зависимостей (Transient)
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class ViewModelEEVMBuilder<TEntity>
		where TEntity : class, IDomainObject {

		#region Обазательные параметры
		protected IEEVMBuilderParameters _parameters;
		protected IPropertyBinder<TEntity> PropertyBinder;
		#endregion

		#region Опциональные компоненты
		protected IEntityAdapter<TEntity> EntityAdapter;
		protected IEntitySelector EntitySelector;
		protected IEntityDlgOpener EntityDlgOpener;
		protected IEntityAutocompleteSelector<TEntity> EntityAutocompleteSelector;
		private DialogViewModelBase _viewModel;
		private IUnitOfWork _unitOfWork;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly INavigationManager _navigationManager;
		#endregion

		public ViewModelEEVMBuilder(ILifetimeScope lifetimeScope, INavigationManager navigationManager) {
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public bool IsParametersCreated => !(_parameters is null) && !(PropertyBinder is null);

		#region Инициализация параметров
		public ViewModelEEVMBuilder<TEntity> SetViewModel<TViewModel>(TViewModel viewModel)
			where TViewModel : DialogViewModelBase {
			if(_viewModel != null) {
				throw new InvalidOperationException("ViewModel уже установлена");
			}
			_viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			TryCreateParameters();
			return this;
		}

		public ViewModelEEVMBuilder<TEntity> SetUnitOfWork(IUnitOfWork unitOfWork) {
			if(_unitOfWork != null) {
				throw new InvalidOperationException("UnitOfWork уже установлен");
			}
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			TryCreateParameters();
			return this;
		}

		private void TryCreateParameters() {
			if(_viewModel is null) {
				return;
			}

			if(_unitOfWork is null) {
				return;
			}

			_parameters = new ViewModelEEVMBuilderParameters(_viewModel, _unitOfWork, _navigationManager, _lifetimeScope);
		}
		#endregion Инициализация параметров

		#region Fluent Config
		public ViewModelEEVMBuilder<TEntity> ForProperty<TBindedEntity>(TBindedEntity bindedEntity, Expression<Func<TBindedEntity, TEntity>> sourceProperty)
			where TBindedEntity : class, INotifyPropertyChanged {
			PropertyBinder = new PropertyBinder<TBindedEntity, TEntity>(bindedEntity, sourceProperty);
			return this;
		}

		public virtual ViewModelEEVMBuilder<TEntity> UseViewModelJournalAndAutocompleter<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase {
			if(!IsParametersCreated) {
				throw new InvalidOperationException("Базовые параметры не установлены");
			}
			EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel>(_parameters.DialogViewModel, _parameters.NavigationManager);
			EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel>(_parameters.AutofacScope);
			return this;
		}

		public virtual ViewModelEEVMBuilder<TEntity> UseViewModelJournalAndAutocompleter<TJournalViewModel, TJournalFilterViewModel>(Action<TJournalFilterViewModel> filterParams)
			where TJournalViewModel : JournalViewModelBase
			where TJournalFilterViewModel : class, IJournalFilterViewModel {
			if(!IsParametersCreated) {
				throw new InvalidOperationException("Базовые параметры не установлены");
			}
			EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel, TJournalFilterViewModel>(_parameters.DialogViewModel, _parameters.NavigationManager, filterParams);
			EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel, TJournalFilterViewModel>(_parameters.AutofacScope, filterParams);
			return this;
		}

		public virtual ViewModelEEVMBuilder<TEntity> UseViewModelJournalAndAutocompleter<TJournalViewModel, TJournalFilterViewModel>(TJournalFilterViewModel filter)
			where TJournalViewModel : JournalViewModelBase
			where TJournalFilterViewModel : class, IJournalFilterViewModel {
			if(!IsParametersCreated) {
				throw new InvalidOperationException("Базовые параметры не установлены");
			}
			EntitySelector = new JournalViewModelSelector<TEntity, TJournalViewModel, TJournalFilterViewModel>(_parameters.DialogViewModel, _parameters.NavigationManager, filter);
			EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel, TJournalFilterViewModel>(_parameters.AutofacScope, filter);
			return this;
		}

		public virtual ViewModelEEVMBuilder<TEntity> UseViewModelDialog<TEntityViewModel>()
			where TEntityViewModel : DialogViewModelBase {
			if(!IsParametersCreated) {
				throw new InvalidOperationException("Базовые параметры не установлены");
			}
			EntityDlgOpener = new EntityViewModelOpener<TEntityViewModel>(_parameters.NavigationManager, _parameters.DialogViewModel);
			return this;
		}

		public virtual ViewModelEEVMBuilder<TEntity> MakeByType() {
			if(!IsParametersCreated) {
				throw new InvalidOperationException("Базовые параметры не установлены");
			}
			if(_parameters.AutofacScope == null)
				throw new NullReferenceException($"{nameof(_parameters.AutofacScope)} не задан для билдера. Без него использование {nameof(MakeByType)} невозможно.");
			var resolver = _parameters.AutofacScope.Resolve<IViewModelResolver>();

			var journalViewModelType = resolver.GetTypeOfViewModel(typeof(TEntity), TypeOfViewModel.Journal);
			if(journalViewModelType != null) {
				var entitySelectorType = typeof(JournalViewModelSelector<,>).MakeGenericType(typeof(TEntity), journalViewModelType);
				EntitySelector = (IEntitySelector)Activator.CreateInstance(entitySelectorType, _parameters.DialogViewModel, _parameters.NavigationManager);

				var entityAutocompleteSelectorType = typeof(JournalViewModelAutocompleteSelector<,>).MakeGenericType(typeof(TEntity), journalViewModelType);
				EntityAutocompleteSelector = (IEntityAutocompleteSelector<TEntity>)Activator.CreateInstance(entityAutocompleteSelectorType, _parameters.AutofacScope);
			}

			var dialogViewModelType = resolver.GetTypeOfViewModel(typeof(TEntity), TypeOfViewModel.EditDialog);
			if(dialogViewModelType != null) {
				var entityDlgOpenerType = typeof(EntityViewModelOpener<>).MakeGenericType(dialogViewModelType);
				EntityDlgOpener = (IEntityDlgOpener)Activator.CreateInstance(entityDlgOpenerType, _parameters.NavigationManager, _parameters.DialogViewModel);
			}
			return this;
		}

		public virtual ViewModelEEVMBuilder<TEntity> UseAdapter(IEntityAdapter<TEntity> adapter) {
			EntityAdapter = adapter;
			return this;
		}

		public virtual ViewModelEEVMBuilder<TEntity> UseFuncAdapter(Func<object, TEntity> getEntityByNode) {
			EntityAdapter = new FuncEntityAdapter<TEntity>(getEntityByNode);
			return this;
		}

		public virtual EntityEntryViewModel<TEntity> Finish() {
			if(!IsParametersCreated) {
				throw new InvalidOperationException("Базовые параметры не установлены");
			}
			var entityAdapter = EntityAdapter ?? new UowEntityAdapter<TEntity>(_parameters.UnitOfWork);
			return new EntityEntryViewModel<TEntity>(PropertyBinder, EntitySelector, EntityDlgOpener, EntityAutocompleteSelector, entityAdapter);
		}
		#endregion Fluent Config

		public class ViewModelEEVMBuilderParameters : IEEVMBuilderParameters {
			public ViewModelEEVMBuilderParameters(
				DialogViewModelBase dialogViewModel,
				IUnitOfWork unitOfWork,
				INavigationManager navigationManager,
				ILifetimeScope autofacScope) {
				DialogViewModel = dialogViewModel ?? throw new ArgumentNullException(nameof(dialogViewModel));
				UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
				NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			}

			public DialogViewModelBase DialogViewModel { get; }
			public IUnitOfWork UnitOfWork { get; }
			public INavigationManager NavigationManager { get; }
			public ILifetimeScope AutofacScope { get; }
		}
	}
}
