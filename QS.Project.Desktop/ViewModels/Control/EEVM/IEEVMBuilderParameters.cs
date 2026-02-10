using Autofac;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;

namespace QS.ViewModels.Control.EEVM
{
	public interface IEEVMBuilderParameters
	{
		IDialogViewModel DialogViewModel { get; }
		IUnitOfWork UnitOfWork { get; }
		INavigationManager NavigationManager { get; }
		ILifetimeScope AutofacScope {get;}
		IEntityChangeWatcher ChangeWatcher { get; }
	}
}
