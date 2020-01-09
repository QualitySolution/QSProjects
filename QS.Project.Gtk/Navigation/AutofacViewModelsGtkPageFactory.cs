using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.ViewModels;

namespace QS.Navigation
{
	public class AutofacViewModelsGtkPageFactory : IViewModelsPageFactory
	{
		readonly IContainer Container;

		public AutofacViewModelsGtkPageFactory(IContainer container)
		{
			Container = container;
		}

		public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, string hash, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var viewmodel = scope.Resolve<TViewModel>(ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));
			if(viewmodel is IAutofacScopeHolder)
				(viewmodel as IAutofacScopeHolder).AutofacScope = scope;
			var page = new GtkWindowPage<TViewModel>(viewmodel, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}

		public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var viewmodel = scope.Resolve<TViewModel>(ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));
			if(viewmodel is IAutofacScopeHolder)
				(viewmodel as IAutofacScopeHolder).AutofacScope = scope;
			var page = new GtkWindowPage<TViewModel>(viewmodel, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}
	}
}
