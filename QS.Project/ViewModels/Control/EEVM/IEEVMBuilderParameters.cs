using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM
{
	public interface IEEVMBuilderParameters
	{
		DialogViewModelBase DialogViewModel { get; }
		IUnitOfWork UnitOfWork { get; }
		INavigationManager NavigationManager { get; }
		ILifetimeScope AutofacScope {get;}
	}
}
