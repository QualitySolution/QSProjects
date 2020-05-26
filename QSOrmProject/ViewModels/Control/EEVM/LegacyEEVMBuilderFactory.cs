using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public class LegacyEEVMBuilderFactory<TBindedEntity> : CommonEEVMBuilderFactory<TBindedEntity>, ILegacyEEVMBuilderParameters
		where TBindedEntity : class, INotifyPropertyChanged
	{
		public Func<ITdiTab> GetDialogTab { get; private set; }

		public LegacyEEVMBuilderFactory(
			DialogViewModelBase dialogViewModel,
			ITdiTab dialogTab,
			TBindedEntity source,
			IUnitOfWork unitOfWork,
			INavigationManager navigation,
			ILifetimeScope autofacScope = null) 
			: base(dialogViewModel, source, unitOfWork, navigation, autofacScope)
		{
			GetDialogTab = () => dialogTab;
		}

		public LegacyEEVMBuilderFactory(
			ITdiTab dialogTab,
			TBindedEntity source,
			IUnitOfWork unitOfWork,
			INavigationManager navigation,
			ILifetimeScope autofacScope = null) 
			: base(null, source, unitOfWork, navigation, autofacScope)
		{
			GetDialogTab = () => dialogTab;
		}

		public new LegacyEEVMBuilder<TPropertyEntity> ForProperty<TPropertyEntity>(Expression<Func<TBindedEntity, TPropertyEntity>> sourceProperty)
			where TPropertyEntity : class, IDomainObject
		{
			var binder = new PropertyBinder<TBindedEntity, TPropertyEntity>(BindedEntity, sourceProperty);
			return new LegacyEEVMBuilder<TPropertyEntity>(this, binder);
		}
	}

	public class LegacyEEVMBuilderFactory : CommonEEVMBuilderFactory, ILegacyEEVMBuilderParameters
	{
		public Func<ITdiTab> GetDialogTab { get; private set; }

		public LegacyEEVMBuilderFactory(
			DialogViewModelBase dialogViewModel,
			ITdiTab dialogTab,
			IUnitOfWork unitOfWork,
			INavigationManager navigation,
			ILifetimeScope autofacScope = null)
			: base(null, unitOfWork, navigation, autofacScope)
		{
			GetDialogTab = () => dialogTab;
		}

		public LegacyEEVMBuilderFactory(
			ITdiTab dialogTab,
			IUnitOfWork unitOfWork,
			INavigationManager navigation,
			ILifetimeScope autofacScope = null)
			: base(null, unitOfWork, navigation, autofacScope)
		{
			GetDialogTab = () => dialogTab;
		}

		public LegacyEEVMBuilderFactory(
			Func<ITdiTab> getDialogTab,
			IUnitOfWork unitOfWork,
			INavigationManager navigation,
			ILifetimeScope autofacScope = null)
			: base(null, unitOfWork, navigation, autofacScope)
		{
			GetDialogTab = getDialogTab;
		}

		public new LegacyEEVMBuilder<TEntity> ForEntity<TEntity>()
			where TEntity : class, IDomainObject
		{
			return new LegacyEEVMBuilder<TEntity>(this);
		}
	}
}