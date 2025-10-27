using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using NUnit.Framework;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace QS.Test.Observable
{
	[TestFixture(TestOf = typeof(ObservableList<>))]
	public class ObservableListTest
	{
		#region Тестовые классы
		
		/// <summary>
		/// Тестовый класс на основе PropertyChangedBase для проверки уведомлений об изменении свойств
		/// </summary>
		private class TestItem : PropertyChangedBase
		{
			private string name;
			public string Name {
				get => name;
				set => SetField(ref name, value);
			}

			private int value;
			public int Value {
				get => this.value;
				set => SetField(ref this.value, value);
			}

			public TestItem(string name, int value = 0)
			{
				this.name = name;
				this.value = value;
			}
		}
		
		#endregion

		[Test(Description = "Проверяем, что событие CollectionChanged срабатывает при добавлении элемента")]
		public void Add_CollectionChangedEventFired()
		{
			// Arrange
			var list = new ObservableList<TestItem>();
			NotifyCollectionChangedEventArgs eventArgs = null;
			list.CollectionChanged += (sender, args) => eventArgs = args;
			var item = new TestItem("Item1");

			// Act
			list.Add(item);

			// Assert
			Assert.That(eventArgs, Is.Not.Null, "Событие CollectionChanged не было вызвано");
			Assert.That(eventArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
			Assert.That(eventArgs.NewItems, Is.Not.Null);
			Assert.That(eventArgs.NewItems[0], Is.EqualTo(item));
			Assert.That(eventArgs.NewStartingIndex, Is.EqualTo(0));
		}

		[Test(Description = "Проверяем, что событие PropertyChanged срабатывает при изменении Count после добавления")]
		public void Add_PropertyChangedEventFired()
		{
			// Arrange
			var list = new ObservableList<TestItem>();
			var propertyChangedList = new List<string>();
			list.PropertyChanged += (sender, args) => propertyChangedList.Add(args.PropertyName);
			var item = new TestItem("Item1");

			// Act
			list.Add(item);

			// Assert
			Assert.That(propertyChangedList, Contains.Item("Count"));
		}

		[Test(Description = "Проверяем, что событие CollectionChanged срабатывает при удалении элемента")]
		public void Remove_CollectionChangedEventFired()
		{
			// Arrange
			var item = new TestItem("Item1");
			var list = new ObservableList<TestItem> { item };
			NotifyCollectionChangedEventArgs eventArgs = null;
			list.CollectionChanged += (sender, args) => eventArgs = args;

			// Act
			list.Remove(item);

			// Assert
			Assert.That(eventArgs, Is.Not.Null, "Событие CollectionChanged не было вызвано");
			Assert.That(eventArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
			Assert.That(eventArgs.OldItems, Is.Not.Null);
			Assert.That(eventArgs.OldItems[0], Is.EqualTo(item));
			Assert.That(eventArgs.OldStartingIndex, Is.EqualTo(0));
		}

		[Test(Description = "Проверяем, что событие PropertyChanged срабатывает при изменении Count после удаления")]
		public void Remove_PropertyChangedEventFired()
		{
			// Arrange
			var item = new TestItem("Item1");
			var list = new ObservableList<TestItem> { item };
			var propertyChangedList = new List<string>();
			list.PropertyChanged += (sender, args) => propertyChangedList.Add(args.PropertyName);

			// Act
			list.Remove(item);

			// Assert
			Assert.That(propertyChangedList, Contains.Item("Count"));
		}

		[Test(Description = "Проверяем, что событие CollectionChanged срабатывает при вставке элемента")]
		public void Insert_CollectionChangedEventFired()
		{
			// Arrange
			var list = new ObservableList<TestItem> { new TestItem("Item1") };
			NotifyCollectionChangedEventArgs eventArgs = null;
			list.CollectionChanged += (sender, args) => eventArgs = args;
			var newItem = new TestItem("Item2");

			// Act
			list.Insert(0, newItem);

			// Assert
			Assert.That(eventArgs, Is.Not.Null, "Событие CollectionChanged не было вызвано");
			Assert.That(eventArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
			Assert.That(eventArgs.NewItems[0], Is.EqualTo(newItem));
			Assert.That(eventArgs.NewStartingIndex, Is.EqualTo(0));
		}

		[Test(Description = "Проверяем, что событие CollectionChanged срабатывает при очистке коллекции")]
		public void Clear_CollectionChangedEventFired()
		{
			// Arrange
			var list = new ObservableList<TestItem> {
				new TestItem("Item1"),
				new TestItem("Item2")
			};
			NotifyCollectionChangedEventArgs eventArgs = null;
			list.CollectionChanged += (sender, args) => eventArgs = args;

			// Act
			list.Clear();

			// Assert
			Assert.That(eventArgs, Is.Not.Null, "Событие CollectionChanged не было вызвано");
			Assert.That(eventArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
		}

		[Test(Description = "Проверяем, что событие CollectionChanged срабатывает при замене элемента через индексатор")]
		public void Indexer_Set_CollectionChangedEventFired()
		{
			// Arrange
			var oldItem = new TestItem("OldItem");
			var list = new ObservableList<TestItem> { oldItem };
			NotifyCollectionChangedEventArgs eventArgs = null;
			list.CollectionChanged += (sender, args) => eventArgs = args;
			var newItem = new TestItem("NewItem");

			// Act
			list[0] = newItem;

			// Assert
			Assert.That(eventArgs, Is.Not.Null, "Событие CollectionChanged не было вызвано");
			Assert.That(eventArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
			Assert.That(eventArgs.NewItems[0], Is.EqualTo(newItem));
			Assert.That(eventArgs.OldItems[0], Is.EqualTo(oldItem));
		}

		[Test(Description = "Проверяем, что при изменении свойства элемента списка срабатывает событие PropertyOfElementChanged")]
		public void ElementPropertyChanged_PropertyOfElementChangedEventFired()
		{
			// Arrange
			var item = new TestItem("Item1", 10);
			var list = new ObservableList<TestItem> { item };
			PropertyChangedEventArgs eventArgs = null;
			object eventSender = null;
			list.PropertyOfElementChanged += (sender, args) => {
				eventSender = sender;
				eventArgs = args;
			};

			// Act
			item.Name = "ChangedName";

			// Assert
			Assert.That(eventArgs, Is.Not.Null, "Событие PropertyOfElementChanged не было вызвано");
			Assert.That(eventSender, Is.EqualTo(item), "Sender события должен быть измененным элементом");
			Assert.That(eventArgs.PropertyName, Is.EqualTo(nameof(TestItem.Name)));
		}

		[Test(Description = "Проверяем, что при изменении свойства элемента списка срабатывает событие ContentChanged")]
		public void ElementPropertyChanged_ContentChangedEventFired()
		{
			// Arrange
			var item = new TestItem("Item1", 10);
			var list = new ObservableList<TestItem> { item };
			bool contentChangedFired = false;
			list.ContentChanged += (sender, args) => contentChangedFired = true;

			// Act
			item.Value = 20;

			// Assert
			Assert.That(contentChangedFired, Is.True, "Событие ContentChanged не было вызвано");
		}

		[Test(Description = "Проверяем, что после удаления элемента из списка событие PropertyOfElementChanged больше не срабатывает")]
		public void Remove_UnsubscribesFromElementPropertyChanged()
		{
			// Arrange
			var item = new TestItem("Item1");
			var list = new ObservableList<TestItem> { item };
			int eventFiredCount = 0;
			list.PropertyOfElementChanged += (sender, args) => eventFiredCount++;

			// Act
			list.Remove(item);
			item.Name = "ChangedName";

			// Assert
			Assert.That(eventFiredCount, Is.EqualTo(0), "Событие PropertyOfElementChanged не должно срабатывать после удаления элемента");
		}

		[Test(Description = "Проверяем, что после очистки списка события PropertyOfElementChanged больше не срабатывают")]
		public void Clear_UnsubscribesFromAllElementsPropertyChanged()
		{
			// Arrange
			var item1 = new TestItem("Item1");
			var item2 = new TestItem("Item2");
			var list = new ObservableList<TestItem> { item1, item2 };
			int eventFiredCount = 0;
			list.PropertyOfElementChanged += (sender, args) => eventFiredCount++;

			// Act
			list.Clear();
			item1.Name = "Changed1";
			item2.Name = "Changed2";

			// Assert
			Assert.That(eventFiredCount, Is.EqualTo(0), "События PropertyOfElementChanged не должны срабатывать после очистки списка");
		}

		[Test(Description = "Проверяем, что подписка на события элементов происходит при создании списка из коллекции")]
		public void Constructor_WithCollection_SubscribesToElements()
		{
			// Arrange
			var item1 = new TestItem("Item1");
			var item2 = new TestItem("Item2");
			var sourceCollection = new List<TestItem> { item1, item2 };
			
			// Act
			var list = new ObservableList<TestItem>(sourceCollection);
			PropertyChangedEventArgs eventArgs = null;
			list.PropertyOfElementChanged += (sender, args) => eventArgs = args;
			item1.Name = "ChangedName";

			// Assert
			Assert.That(eventArgs, Is.Not.Null, "Событие PropertyOfElementChanged должно срабатывать для элементов из исходной коллекции");
			Assert.That(eventArgs.PropertyName, Is.EqualTo(nameof(TestItem.Name)));
		}

		[Test(Description = "Проверяем, что ContentChanged срабатывает при добавлении элемента")]
		public void Add_ContentChangedEventFired()
		{
			// Arrange
			var list = new ObservableList<TestItem>();
			bool contentChangedFired = false;
			list.ContentChanged += (sender, args) => contentChangedFired = true;

			// Act
			list.Add(new TestItem("Item1"));

			// Assert
			Assert.That(contentChangedFired, Is.True, "Событие ContentChanged должно срабатывать при добавлении");
		}

		[Test(Description = "Проверяем работу с элементами, не реализующими INotifyPropertyChanged")]
		public void NonNotifyingElements_NoPropertyChangedEvents()
		{
			// Arrange
			var list = new ObservableList<string>();
			int propertyChangedCount = 0;
			list.PropertyOfElementChanged += (sender, args) => propertyChangedCount++;

			// Act
			list.Add("Test");
			list.Remove("Test");

			// Assert
			Assert.That(propertyChangedCount, Is.EqualTo(0), "PropertyOfElementChanged не должно срабатывать для элементов без INotifyPropertyChanged");
			Assert.That(list.Count, Is.EqualTo(0));
		}

		[Test(Description = "Проверяем множественные изменения элементов в списке")]
		public void MultipleElementChanges_AllEventsFired()
		{
			// Arrange
			var item1 = new TestItem("Item1", 1);
			var item2 = new TestItem("Item2", 2);
			var list = new ObservableList<TestItem> { item1, item2 };
			var changedProperties = new List<string>();
			list.PropertyOfElementChanged += (sender, args) => changedProperties.Add(args.PropertyName);

			// Act
			item1.Name = "Changed1";
			item2.Value = 20;
			item1.Value = 10;

			// Assert
			Assert.That(changedProperties.Count, Is.EqualTo(3));
			Assert.That(changedProperties, Contains.Item(nameof(TestItem.Name)));
			Assert.That(changedProperties, Contains.Item(nameof(TestItem.Value)));
		}
	}
}

