using System;
using System.ComponentModel;
using System.Linq.Expressions;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public class LegacyEEVMBuilderFactory<TBindedEntity> : CommonEEVMBuilderFactory<TBindedEntity>
		where TBindedEntity : class, INotifyPropertyChanged
	{
		public readonly ITdiTab DialogTab;

		public LegacyEEVMBuilderFactory(DialogViewModelBase dialogViewModel, ITdiTab dialogTab, TBindedEntity source, IUnitOfWork unitOfWork, INavigationManager navigation) : base(dialogViewModel, source, unitOfWork, navigation)
		{
			DialogTab = dialogTab;
		}

		public new LegacyEEVMBuilder<TBindedEntity, TPropertyEntity> ForProperty<TPropertyEntity>(Expression<Func<TBindedEntity, TPropertyEntity>> sourceProperty)
			where TPropertyEntity : class, IDomainObject
		{
			return new LegacyEEVMBuilder<TBindedEntity, TPropertyEntity>(this, sourceProperty);
		}
	}
}