using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Tdi;

namespace QS.Navigation
{
	public class AutofacTdiPageFactory : ITdiPageFactory
	{
		readonly IContainer Container;

		public AutofacTdiPageFactory(IContainer container)
		{
			Container = container;
		}

		public IPage CreateTdiPageNamedArgs<TTdiTab>(IDictionary<string, object> ctorArgs, string hash) where TTdiTab : ITdiTab
		{
			var scope = Container.BeginLifetimeScope();
			var tab = scope.Resolve<TTdiTab>(ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));
			if(tab is IAutofacScopeHolder)
				(tab as IAutofacScopeHolder).AutofacScope = scope;
			var page = new TdiTabPage(tab, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}

		public IPage CreateTdiPageTypedArgs<TTdiTab>(Type[] ctorTypes, object[] ctorValues, string hash) where TTdiTab : ITdiTab
		{
			var scope = Container.BeginLifetimeScope();
			var tab = scope.Resolve<TTdiTab>(ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));
			if(tab is IAutofacScopeHolder)
				(tab as IAutofacScopeHolder).AutofacScope = scope;
			var page = new TdiTabPage(tab, hash);
			page.PageClosed += (sender, e) => scope.Dispose();
			return page;
		}
	}
}
