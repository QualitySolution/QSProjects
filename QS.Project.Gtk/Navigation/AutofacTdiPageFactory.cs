using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Tdi;

namespace QS.Navigation
{
	public class AutofacTdiPageFactory : ITdiPageFactory
	{
		readonly ILifetimeScope Container;

		public AutofacTdiPageFactory(ILifetimeScope container)
		{
			Container = container;
		}

		public IPage CreateTdiPageNamedArgs<TTdiTab>(
			IDictionary<string, object> ctorArgs,
			string hash,
			Action<TTdiTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTdiTab : ITdiTab
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var tab = scope.Resolve<TTdiTab>(ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));
			configureTab?.Invoke(tab);
			
			var page = new TdiTabPage(tab, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}

		public IPage CreateTdiPageTypedArgs<TTdiTab>(
			Type[] ctorTypes,
			object[] ctorValues,
			string hash,
			Action<TTdiTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTdiTab : ITdiTab
		{
			var scope = addingRegistrations == null ? Container.BeginLifetimeScope() : Container.BeginLifetimeScope(addingRegistrations);
			var tab = scope.Resolve<TTdiTab>(ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));
			configureTab?.Invoke(tab);
			
			var page = new TdiTabPage(tab, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}
	}
}
