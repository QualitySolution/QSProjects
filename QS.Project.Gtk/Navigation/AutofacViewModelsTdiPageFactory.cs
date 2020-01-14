using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Tdi;
using QS.ViewModels.Dialog;
using QS.Views.Resolve;

namespace QS.Navigation
{
	public class AutofacViewModelsTdiPageFactory : IViewModelsPageFactory
	{
		readonly IContainer Container;

		public AutofacViewModelsTdiPageFactory(IContainer container)
		{
			Container = container;
		}

		public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, string hash, Action<ContainerBuilder> addingRegistrations) where TViewModel : DialogViewModelBase
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var viewmodel = scope.Resolve<TViewModel>(ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));
			return MakePage(viewmodel, scope, hash);
		}

		public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash, Action<ContainerBuilder> addingRegistrations) where TViewModel : DialogViewModelBase
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var viewmodel = scope.Resolve<TViewModel>(ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));
			return MakePage(viewmodel, scope, hash);
		}

		private IPage<TViewModel> MakePage<TViewModel>(TViewModel viewmodel, ILifetimeScope scope, string hash) where TViewModel : DialogViewModelBase
		{
			if (viewmodel is IAutofacScopeHolder)
				(viewmodel as IAutofacScopeHolder).AutofacScope = scope;
			ITdiTab tab;
			if (viewmodel is ITdiTab tdiTab)
				tab = tdiTab;
			else {
				var resolver = scope.Resolve<IGtkViewResolver>();
				var view = resolver.Resolve(viewmodel);
				tab = new ViewModelTdiTab(viewmodel, view);
			}
			var page = new TdiPage<TViewModel>(viewmodel, tab, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}
	}
}
