using System;
using NHibernate.Collection.Generic;
using NHibernate.Engine;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace QS.Extensions.Observable.Collections.List {

	/// <summary>
	/// Represents a persistent wrapper for a generic bag collection that fires events when the
	/// collection's contents have changed.
	/// </summary>
	/// <typeparam name="T">Type of item to be stored in the list.</typeparam>
	public class PersistentGenericObservableBag<T> : PersistentGenericBag<T>, IObservableList<T>, IObservableList, IReadOnlyCollection<T>, IReadOnlyList<T> {

		public event PropertyChangedEventHandler PropertyChanged;

		public PersistentGenericObservableBag(ISessionImplementor session, ObservableList<T> list) : base(session, list) {
			SubscribesAll();
		}

		public PersistentGenericObservableBag(ISessionImplementor session) : base(session) {
		}

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
			ClearSubscribes();
			base.Clear();
			OnCollectionReset();
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
		
		public int RemoveAll(Predicate<T> match) {
			int num = 0;
			for(int i = Count - 1; i >= 0 ; i--) {
				if(match(base[i])) {
					base.RemoveAt(i);
					num++;
				}
			}
			return num;
		}

		#region INotifyCollectionChanged Members

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

		#region PropertyOfElementChanged

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

		private void SubscribesAll() {
			foreach(var item in this) {
				SubscribeElementChanged(item);
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
