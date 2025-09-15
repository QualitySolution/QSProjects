using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DomainModel.Entity {
	/// <summary>
	/// Read-only ковариантный интерфейс коллекции. Параметр T помечен как <c>out</c>,
	/// поэтому реализация может быть присвоена переменной более общего типа:
	/// <c>ICovariantCollection&lt;Derived&gt;</c> можно использовать как
	/// <c>ICovariantCollection&lt;Base&gt;</c> если <c>Derived : Base</c>.
	/// 
	/// Этот интерфейс НЕ содержит методов мутации — он предназначен только для чтения
	/// и перечисления, чтобы безопасно экспонировать коллекцию как ковариантную.
	/// </summary>
	/// <typeparam name="T">Тип элементов (ковариантный).</typeparam>
	public interface ICovariantCollection<out T> : IEnumerable<T> {
		/// <summary>Количество элементов в коллекции.</summary>
		int Count { get; }

		// Методы мутации принимают object чтобы интерфейс оставался ковариантным (out T).
		// Реализация обязана выполнять приведение и выбрасывать InvalidCastException
		// при несовместимости типов.

		/// <summary>Добавляет элемент (выполнение приведения внутри реализации).</summary>
		void Add(object item);

		/// <summary>Вставляет элемент на позицию (выполнение приведения внутри реализации).</summary>
		void Insert(int index, object item);

		/// <summary>Удаляет первое вхождение элемента (после приведения).</summary>
		bool Remove(object item);

		/// <summary>Удаляет элемент по индексу.</summary>
		void RemoveAt(int index);

		/// <summary>Очищает коллекцию.</summary>
		void Clear();

		/// <summary>Проверяет наличие элемента (после приведения).</summary>
		bool Contains(object item);

		/// <summary>Индекс первого вхождения или -1.</summary>
		int IndexOf(object item);

		/// <summary>Копирует элементы в массив object начиная с указанного индекса.</summary>
		void CopyTo(object[] array, int arrayIndex);

		/// <summary>Индексатор только для чтения — возвращает элемент как T.</summary>
		T this[int index] { get; }
	}
}
