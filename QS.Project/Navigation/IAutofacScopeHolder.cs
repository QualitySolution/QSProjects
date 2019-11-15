using System;
using Autofac;

namespace QS.Navigation
{
	/// <summary>
	/// Интерфейс для классов которые хотят иметь ссылку на ILifetimeScope. Например ViewModel которая хочет создавать 
	/// объекты через Autofac.
	/// </summary>
	public interface IAutofacScopeHolder
	{
		ILifetimeScope AutofacScope { get; set; }
	}
}
