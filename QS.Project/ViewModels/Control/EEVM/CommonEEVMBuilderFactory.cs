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
	public class CommonEEVMBuilderFactory<TBindedEntity>
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

		public CommonEEVMBuilderFactory(DialogViewModelBase dialogViewModel, TBindedEntity source, IUnitOfWork unitOfWork, INavigationManager navigation)
		{
			BindedEntity = source;
			DialogViewModel = dialogViewModel;
			UnitOfWork = unitOfWork;
			NavigationManager = navigation;
		}

		public CommonEEVMBuilder<TBindedEntity, TPropertyEntity> ForProperty<TPropertyEntity>(Expression<Func<TBindedEntity, TPropertyEntity>> sourceProperty)
			where TPropertyEntity : class, IDomainObject
		{
			return new CommonEEVMBuilder<TBindedEntity, TPropertyEntity>(this, sourceProperty);
		}
	}
}
