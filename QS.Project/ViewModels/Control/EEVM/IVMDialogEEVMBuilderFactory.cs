using System;
using QS.DomainModel.UoW;
using QS.Navigation;

namespace QS.ViewModels.Control.EEVM
{
	public interface IVMDialogEEVMBuilderFactory<TBindedEntity>
	{
		TBindedEntity BindedEntity { get; }
		ViewModelBase DialogViewModel { get; }
		IUnitOfWork UnitOfWork { get; }
		INavigationManager NavigationManager { get; }
	}
}
