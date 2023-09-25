using System;
using System.ComponentModel;

namespace QS.Extensions.Observable.Collections.List {

	/// <summary>
	/// Notify then property of collection element changed
	/// </summary>
	public interface INotifyCollectionElementChanged 
	{
		/// <summary>
		/// Вызывается при изменении свойства элемента коллекции
		/// </summary>
		event PropertyChangedEventHandler PropertyOfElementChanged;
		/// <summary>
		/// Вызывается при любом изменении содержимого коллекции,
		/// как свойств ее элементов, так и при добавлении/удалении элементов.
		/// </summary>
		event EventHandler ContentChanged;
	}
}
