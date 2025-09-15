
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QS.DomainModel.Entity {
	/// <summary>
	/// Изменяемая коллекция на добавление и удаление, но не обновление, которая хранит элементы типа <typeparamref name="T"/>.
	/// Класс реализует ковариантный вид <see cref="ICovariantCollection{T}"/>. 
	/// Благодаря этому можно передать
	/// экземпляр CovariantList of Type Derived в API, принимающее
	/// ICovariantCollection of Type Base без копирования.
	/// 
	/// Может быть опасно при невнимательном добавлении иного по сути типа нежели коллекция с общим интерфесом.
	/// </summary>
	public class CovariantCollection<T> : ICovariantCollection<T>
	{
		private T[] _items;
		private int _size;
		private const int DefaultCapacity = 4;

		public CovariantCollection() {
			_items = Array.Empty<T>();
			_size = 0;
		}

		public CovariantCollection(int capacity) {
			if(capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			_items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
			_size = 0;
		}

		/// <summary>
		/// Создаёт коллекцию, скопировав элементы из <paramref name="source"/>.
		/// Оптимизировано: если <paramref name="source"/> уже <see cref="CovariantCollection{T}"/>,
		/// возвращается тот же экземпляр (копирование не производится). Если источник
		/// реализует <see cref="ICollection{T}"/>, резервируется нужная ёмкость и
		/// выполняется одно проходное копирование.
		/// </summary>
		public CovariantCollection(IEnumerable<T> source) {
			if(source == null) 
				throw new ArgumentNullException(nameof(source));

			if(source is CovariantCollection<T> existing) {
				_items = existing._items;
				_size = existing._size;
				return;
			}

			if(source is ICollection<T> col) {
				_items = col.Count == 0 ? Array.Empty<T>() : new T[col.Count];
				_size = 0;
				foreach(var v in col) _items[_size++] = v;
				return;
			}

			_items = Array.Empty<T>();
			_size = 0;
			foreach(var v in source) Add(v);
		}

		private void EnsureCapacity(int min) {
			if(_items.Length < min) {
				int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
				if(newCapacity < min) newCapacity = min;
				Array.Resize(ref _items, newCapacity);
			}
		}

		public int Count => _size;

		public T this[int index] {
			get {
				if((uint)index >= (uint)_size) throw new ArgumentOutOfRangeException(nameof(index));
				return _items[index];
			}
		}

		public void Add(object item) {
			if(item == null) throw new ArgumentNullException(nameof(item));
			if(item is T t) {
				EnsureCapacity(_size + 1);
				_items[_size++] = t;
				return;
			}
			throw new InvalidCastException($"элемент типа {item.GetType()} не может быть вставлен в коллекцию типа {typeof(T)}");
		}

		public void Insert(int index, object item) {
			if(item == null) throw new ArgumentNullException(nameof(item));
			if((uint)index > (uint)_size) throw new ArgumentOutOfRangeException(nameof(index));
			if(!(item is T t)) throw new InvalidCastException($"элемент типа {item.GetType()} не может быть вставлен в коллекцию типа {typeof(T)}");
			EnsureCapacity(_size + 1);
			if(index < _size) Array.Copy(_items, index, _items, index + 1, _size - index);
			_items[index] = t;
			_size++;
		}

		public bool Remove(object item) {
			if(item is T t) {
				int idx = IndexOf(t);
				if(idx >= 0) {
					RemoveAt(idx);
					return true;
				}
			}
			return false;
		}

		public void RemoveAt(int index) {
			if((uint)index >= (uint)_size) throw new ArgumentOutOfRangeException(nameof(index));
			_size--;
			if(index < _size) Array.Copy(_items, index + 1, _items, index, _size - index);
			_items[_size] = default!;
		}

		public void Clear() {
			if(_size > 0) {
				Array.Clear(_items, 0, _size);
				_size = 0;
			}
		}

		public bool Contains(object item) {
			if(item is T t) return IndexOf(t) >= 0;
			return false;
		}

		public int IndexOf(object item) {
			if(item is T t) {
				return Array.IndexOf(_items, t, 0, _size);
			}
			return -1;
		}

		public void CopyTo(object[] array, int arrayIndex) {
			if(array == null) throw new ArgumentNullException(nameof(array));
			if(arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if(array.Length - arrayIndex < _size) throw new ArgumentException("недостаточно большой массив");
			Array.Copy(_items, 0, array, arrayIndex, _size);
		}
		public IEnumerator<T> GetEnumerator() {
			for(int i = 0; i < _size; i++) yield return _items[i];
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}


