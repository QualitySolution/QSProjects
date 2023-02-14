using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Testing.Testing.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	public class AutofacViewModelsFactoryForTests : IViewModelsPageFactory
	{
		//Здесь используется ILifetimeScope вместо IContainer, чтобы можно было создать
		//скопы с замененным менеджером навигации, переопределенным только для отдельного скопа.
		readonly ILifetimeScope Container;

		public AutofacViewModelsFactoryForTests(ILifetimeScope container)
		{
			Container = container;
		}

		public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, string hash, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var viewmodel = scope.Resolve<TViewModel>(ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));

			var page = new PageForTests<TViewModel>(viewmodel);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}

		public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var viewmodel = scope.Resolve<TViewModel>(ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));

			var page = new PageForTests<TViewModel>(viewmodel);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}
	}
}
