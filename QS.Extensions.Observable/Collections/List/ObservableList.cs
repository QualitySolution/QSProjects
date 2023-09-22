using System;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.UserTypes;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace QS.Extensions.Observable.Collections.List 
{

	/// <summary>
	/// Represents a list collection that fires events when the collection's contents have 
	/// changed.
	/// </summary>
	/// <typeparam name="T">Type of item to be stored in the list.</typeparam>
	public class ObservableList<T> : List<T>, IObservableList<T>, IObservableList, IUserCollectionType, IReadOnlyCollection<T>, IReadOnlyList<T> {
		#region Конструкторы
		public ObservableList() { }
		public ObservableList(int capacity) : base(capacity) { }

		public ObservableList(IEnumerable<T> collection) : base(collection) {
			foreach(var item in collection)
				SubscribeElementChanged(item);
		}
		#endregion

		/// <summary>
		/// Вызывается при изменении свойства Count
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		public new T this[int index] { 
			get { return base[index]; } 
			set {
				var oldItem = base[index];
				base[index] = value;
				OnItemReplaced(value, oldItem, index);
				SubscribeElementChanged(value);
			}
		}

		public new void Add(T item) {
			base.Add(item);
			OnItemAdded(item);
			SubscribeElementChanged(item);
		}

		public new void Clear() {
			base.Clear();
			OnCollectionReset();
			ClearSubscribes();
		}

		public new void Insert(int index, T item) {
			base.Insert(index, item);
			OnItemInserted(index, item);
			SubscribeElementChanged(item);
		}

		public new bool Remove(T item) {
			int index = IndexOf(item);

			bool result = base.Remove(item);
			OnItemRemoved(item, index);
			UnsubscribeElementChanged(item);

			return result;
		}

		public new void RemoveAt(int index) {
			T item = this[index];

			base.RemoveAt(index);
			OnItemRemoved(item, index);
			UnsubscribeElementChanged(item);
		}

		#region INotifyCollectionChanged Members
		/// <summary>
		/// Вызывается при изменении коллекции: добавление, удаление, замена элементов.
		/// Не вызывается при изменении свойств элементов коллекции.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Fires the <see cref="CollectionChanged"/> event to indicate an item has been 
		/// added to the end of the collection.
		/// </summary>
		/// <param name="item">Item added to the collection.</param>
		protected void OnItemAdded(T item) {
			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1);
			CollectionChanged?.Invoke(this, args);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			ContentChanged?.Invoke(this, args);
		}

		/// <summary>
		/// Fires the <see cref="CollectionChanged"/> event to indicate the collection 
		/// has been reset.  This is used when the collection has been cleared or 
		/// entirely replaced.
		/// </summary>
		protected void OnCollectionReset() {
			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			CollectionChanged?.Invoke(this, args);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			ContentChanged?.Invoke(this, args);
		}

		/// <summary>
		/// Fires the <see cref="CollectionChanged"/> event to indicate an item has 
		/// been inserted into the collection at the specified index.
		/// </summary>
		/// <param name="index">Index the item has been inserted at.</param>
		/// <param name="item">Item inserted into the collection.</param>
		protected void OnItemInserted(int index, T item) {
			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
			CollectionChanged?.Invoke(this, args);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			ContentChanged?.Invoke(this, args);
		}

		/// <summary>
		/// Fires the <see cref="CollectionChanged"/> event to indicate an item has
		/// been removed from the collection at the specified index.
		/// </summary>
		/// <param name="item">Item removed from the collection.</param>
		/// <param name="index">Index the item has been removed from.</param>
		protected void OnItemRemoved(T item, int index) {
			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
			CollectionChanged?.Invoke(this, args);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			ContentChanged?.Invoke(this, args);
		}

		/// <summary>
		/// Fires the <see cref="CollectionChanged"/> event to indicate an item has been 
		/// replaced.
		/// </summary>
		/// <param name="newItem">Item what replacing <paramref name="oldItem"/></param>
		/// <param name="oldItem">Item what replaced by <paramref name="newItem"/></param>
		/// <param name="index">Replaced item index</param>
		protected void OnItemReplaced(T newItem, T oldItem, int index) {
			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index);
			CollectionChanged?.Invoke(this, args);
			ContentChanged?.Invoke(this, args);
		}

		#endregion

		#region IUserCollectionType Members

		public bool Contains(object collection, object entity) {
			return ((IObservableList<T>)collection).Contains((T)entity);
		}

		public new IEnumerable GetElements(object collection) {
			return (IEnumerable)collection;
		}

		public object IndexOf(object collection, object entity) {
			return ((IObservableList<T>)collection).IndexOf((T)entity);
		}

		public object Instantiate() {
			return new ObservableList<T>();
		}

		public object Instantiate(int anticipatedSize) {
			return Instantiate();
		}

		public IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister) {
			return new PersistentGenericObservableBag<T>(session);
		}

		public object ReplaceElements(object original, object target, ICollectionPersister persister, object owner, IDictionary copyCache, ISessionImplementor session) {
			IObservableList<T> result = (IObservableList<T>)target;

			result.Clear();
			foreach(object item in (IEnumerable)original) {
				result.Add((T)item);
			}
			OnCollectionReset();

			return result;
		}

		public IPersistentCollection Wrap(ISessionImplementor session, object collection) {
			return new PersistentGenericObservableBag<T>(session, (ObservableList<T>)collection);
		}

		#endregion

		#region PropertyOfElementChanged

		/// <summary>
		///	Вызывается при изменении свойства элемента коллекции.
		/// </summary>
		public event PropertyChangedEventHandler PropertyOfElementChanged;

		public event EventHandler ContentChanged;

		private void SubscribeElementChanged(T element) {
			if(element is INotifyPropertyChanged notifiedElement) {
				notifiedElement.PropertyChanged += NotifiedElement_PropertyChanged;
			}
		}

		private void UnsubscribeElementChanged(T element) {
			if(element is INotifyPropertyChanged notifiedElement) {
				notifiedElement.PropertyChanged -= NotifiedElement_PropertyChanged;
			}
		}

		private void ClearSubscribes() {
			foreach(var item in this) {
				UnsubscribeElementChanged(item);
			}
		}

		private void NotifiedElement_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			PropertyOfElementChanged?.Invoke(sender, e);
			ContentChanged?.Invoke(sender, e);
		}

		#endregion
	}
}
