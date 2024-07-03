using System;
using System.Collections.Generic;
using Autofac;
using QS.Tdi;

namespace QS.Navigation
{
	public interface ITdiPageFactory
	{
		IPage CreateTdiPageTypedArgs<TTdiTab>(
			Type[] ctorTypes,
			object[] ctorValues,
			string hash,
			Action<TTdiTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTdiTab : ITdiTab;

		IPage CreateTdiPageNamedArgs<TTdiTab>(
			IDictionary<string, object> ctorArgs,
			string hash,
			Action<TTdiTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTdiTab : ITdiTab;
	}
}
