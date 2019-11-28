using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.ViewModels;

namespace QS.Navigation
{
	public class AutofacViewModelsPageFactory : IViewModelsPageFactory
	{
		readonly IContainer Container;

		public AutofacViewModelsPageFactory(IContainer container)
		{
			Container = container;
		}

		public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, string hash) where TViewModel : ViewModelBase
		{
			var scope = Container.BeginLifetimeScope();
			var viewmodel = scope.Resolve<TViewModel>(ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));
			if(viewmodel is IAutofacScopeHolder)
				(viewmodel as IAutofacScopeHolder).AutofacScope = scope;
			var page = new Page<TViewModel>(viewmodel, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}

		public IPage CreateViewModelNamedArgs(Type viewModelType, ViewModelBase master, IDictionary<string, object> ctorArgs, string hash)
		{
			if(!viewModelType.IsSubclassOf(typeof(ViewModelBase))) {
				throw new InvalidOperationException($"{nameof(viewModelType)} должен наследоваться от {nameof(ViewModelBase)}");
			}
			var scope = Container.BeginLifetimeScope();
			ViewModelBase viewmodel = (ViewModelBase)scope.Resolve(viewModelType, ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));
			if(viewmodel is IAutofacScopeHolder)
				(viewmodel as IAutofacScopeHolder).AutofacScope = scope;
			var page = new Page<ViewModelBase>(viewmodel, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}

		public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash) where TViewModel : ViewModelBase
		{
			var scope = Container.BeginLifetimeScope();
			var viewmodel = scope.Resolve<TViewModel>(ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));
			if(viewmodel is IAutofacScopeHolder)
				(viewmodel as IAutofacScopeHolder).AutofacScope = scope;
			var page = new Page<TViewModel>(viewmodel, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}

		public IPage CreateViewModelTypedArgs(Type viewModelType, ViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash)
		{
			if(!viewModelType.IsSubclassOf(typeof(ViewModelBase))) {
				throw new InvalidOperationException($"{nameof(viewModelType)} должен наследоваться от {nameof(ViewModelBase)}");
			}
			var scope = Container.BeginLifetimeScope();
			ViewModelBase viewmodel = (ViewModelBase)scope.Resolve(viewModelType, ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));
			if(viewmodel is IAutofacScopeHolder)
				(viewmodel as IAutofacScopeHolder).AutofacScope = scope;
			var page = new Page<ViewModelBase>(viewmodel, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}
	}
}
