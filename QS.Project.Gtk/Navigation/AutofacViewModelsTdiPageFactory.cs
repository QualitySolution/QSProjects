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
		readonly ILifetimeScope Container;

		public AutofacViewModelsTdiPageFactory(ILifetimeScope container)
		{
			Container = container;
		}

		public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, string hash, Action<ContainerBuilder> addingRegistrations) where TViewModel : DialogViewModelBase
		{
			var parameters = ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value));
			return MakePage<TViewModel>(parameters, hash, addingRegistrations);
		}

		public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash, Action<ContainerBuilder> addingRegistrations) where TViewModel : DialogViewModelBase
		{
			var parameters = ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val));
			return MakePage<TViewModel>(parameters, hash, addingRegistrations);
		}

		private IPage<TViewModel> MakePage<TViewModel>(IEnumerable<Autofac.Core.Parameter> parameters, string hash, Action<ContainerBuilder> addingRegistrations) where TViewModel : DialogViewModelBase
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			ViewModelTdiTab viewModelTab = null;
			ITdiTab tab;
			var args = parameters.ToList();

			if(!typeof(TViewModel).IsAssignableTo<ITdiTab>()) {
				if(typeof(TViewModel).IsAssignableTo<ISlideableViewModel>())
					viewModelTab = new ViewModelTdiJournalTab();
				else
					viewModelTab = new ViewModelTdiTab();
			}

			if(typeof(TViewModel).IsAssignableTo<ILegacyViewModel>()) {
				args.Add(new NamedParameter("myTab", viewModelTab));
			}

			var viewmodel = scope.Resolve<TViewModel>(args);
			if (viewmodel is ITdiTab tdiTab)
				tab = tdiTab;
			else {
				var resolver = scope.Resolve<IGtkViewResolver>();
				var view = resolver.Resolve(viewmodel);
				viewModelTab.Setup(viewmodel, view);
				tab = viewModelTab;
			}
			var page = new TdiPage<TViewModel>(viewmodel, tab, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}
	}
}
