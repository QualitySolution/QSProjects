using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	/// <summary>
	/// Строитель позволяющий просто создавать EntityEntryViewModel-и с биндингом на свойства класса TBindedEntity
	/// </summary>
	public class CommonEEVMBuilderFactory<TBindedEntity> : IEEVMBuilderParameters
		where TBindedEntity: class, INotifyPropertyChanged
	{
		public TBindedEntity BindedEntity { get; set; }
		public DialogViewModelBase DialogViewModel { get; set; }
		public IUnitOfWork UnitOfWork { get; set; }
		public INavigationManager NavigationManager { get; set; }

		/// <summary>
		/// Необходимо пока только для Autocompletion, пока не стал добавлять в обязательные поля.
		/// </summary>
		public ILifetimeScope AutofacScope { get; set; }

		public CommonEEVMBuilderFactory(DialogViewModelBase dialogViewModel, TBindedEntity source, IUnitOfWork unitOfWork, INavigationManager navigation, ILifetimeScope autofacScope = null)
		{
			BindedEntity = source;
			DialogViewModel = dialogViewModel;
			UnitOfWork = unitOfWork;
			NavigationManager = navigation;
			AutofacScope = autofacScope;
		}

		public CommonEEVMBuilder<TPropertyEntity> ForProperty<TPropertyEntity>(Expression<Func<TBindedEntity, TPropertyEntity>> sourceProperty)
			where TPropertyEntity : class, IDomainObject
		{
			var binder = new PropertyBinder<TBindedEntity, TPropertyEntity>(BindedEntity, sourceProperty);
			return new CommonEEVMBuilder<TPropertyEntity>(this, binder);
		}

		public void Dispose() {
			BindedEntity = null;
			DialogViewModel = null;
			UnitOfWork = null;
			NavigationManager = null;
			AutofacScope = null;
		}
	}

	/// <summary>
	/// Строитель позволяющий просто создавать EntityEntryViewModel-и без биндинга к классам.
	/// </summary>
	public class CommonEEVMBuilderFactory : IEEVMBuilderParameters
	{
		public DialogViewModelBase DialogViewModel { get; set; }
		public IUnitOfWork UnitOfWork { get; set; }
		public INavigationManager NavigationManager { get; set; }

		/// <summary>
		/// Необходимо пока только для Autocompletion, пока не стал добавлять в обязательные поля.
		/// </summary>
		public ILifetimeScope AutofacScope { get; set; }

		public CommonEEVMBuilderFactory(DialogViewModelBase dialogViewModel, IUnitOfWork unitOfWork, INavigationManager navigation, ILifetimeScope autofacScope = null)
		{
			DialogViewModel = dialogViewModel;
			UnitOfWork = unitOfWork;
			NavigationManager = navigation;
			AutofacScope = autofacScope;
		}

		public CommonEEVMBuilder<TEntity> ForEntity<TEntity>()
			where TEntity : class, IDomainObject
		{
			return new CommonEEVMBuilder<TEntity>(this);
		}
	}
}
