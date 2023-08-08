using NHibernate.UserTypes;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace QS.Extensions.Observable.Collections.List {
	/// <summary>
	/// Outlines the behavior that the user and persistent notifying list collections 
	/// will exhibit.
	/// </summary>
	/// <remarks>
	/// When using this collection type in an application with an NHibernate back-end, this 
	/// interface should be used as the declaring type for any class members in which the 
	/// desired type is <see cref="ObservableList<T>"/>.  NHibernate will provide an instance
	/// of the <see cref="PersistentGenericObservableBag<T>"/> persistent wrapper class if the
	/// collection has been persisted.
	/// </remarks>
	/// <typeparam name="T">Type of item to be stored in the list.</typeparam>
	public interface IObservableList<T> : IList<T>, INotifyCollectionChanged, INotifyCollectionElementChanged { }

	public interface IObservableList : IList, INotifyCollectionChanged, INotifyCollectionElementChanged { }
}
