using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using QS.Test.TestApp.Domain;
using QS.Testing.DB;

namespace QS.Test.Observable
{
	[TestFixture]
	[NUnit.Framework.Category("Integrated")]
	public class ObservableListIntegratedTest : InMemoryDBTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			InitialiseNHibernate(typeof(EntityWithObservableList).Assembly);
		}

		[Test(Description = "Проверяем, что коллекция, загруженная через NHibernate, реагирует на изменения свойств элементов")]
		public void LoadedCollection_ElementPropertyChanged_EventsFired()
		{
			NewSessionWithSameDB();
			// Должен оставаться открытым до завершения теста, чтобы база не удалилась из памяти
			using var uow = UnitOfWorkFactory.Create();
			
			// Arrange - создаем и сохраняем сущность с коллекцией
			var entity = new EntityWithObservableList("Test Entity");
			entity.Items.Add(new TestItem("Item 1", 10));
			entity.Items.Add(new TestItem("Item 2", 20));
			entity.Items.Add(new TestItem("Item 3", 30));

			uow.Save(entity);
			uow.Commit();

			// Act - загружаем сущность в новой сессии и подписываемся на события
			using (var uow2 = UnitOfWorkFactory.Create()) {
				var loadedEntity = uow2.GetById<EntityWithObservableList>(entity.Id);

				// Проверяем, что коллекция загрузилась
				Assert.That(loadedEntity.Items.Count, Is.EqualTo(3));

				PropertyChangedEventArgs propertyChangedArgs = null;
				object propertyChangedSender = null;
				bool contentChangedFired = false;

				// Подписываемся на события коллекции
				loadedEntity.Items.PropertyOfElementChanged += (sender, args) => {
					propertyChangedSender = sender;
					propertyChangedArgs = args;
				};

				loadedEntity.Items.ContentChanged += (sender, args) => {
					contentChangedFired = true;
				};

				// Изменяем свойство элемента
				var firstItem = loadedEntity.Items.First();
				firstItem.Name = "Changed Item 1";

				// Assert
				Assert.That(propertyChangedArgs, Is.Not.Null, "Событие PropertyOfElementChanged не было вызвано");
				Assert.That(propertyChangedSender, Is.EqualTo(firstItem), "Sender события должен быть измененным элементом");
				Assert.That(propertyChangedArgs.PropertyName, Is.EqualTo(nameof(TestItem.Name)));
				Assert.That(contentChangedFired, Is.True, "Событие ContentChanged не было вызвано");
			}
		}

		[Test(Description = "Проверяем, что коллекция, загруженная через NHibernate, реагирует на добавление элементов")]
		public void LoadedCollection_AddItem_CollectionChangedEventFired() {
			NewSessionWithSameDB();
			// Должен оставаться открытым до завершения теста, чтобы база не удалилась из памяти
			using var uow = UnitOfWorkFactory.Create();

			// Arrange
			var entity = new EntityWithObservableList("Test Entity");
			entity.Items.Add(new TestItem("Item 1", 10));

			uow.Save(entity);
			uow.Commit();

			// Act
			using(var uow2 = UnitOfWorkFactory.Create()) {
				var loadedEntity = uow2.GetById<EntityWithObservableList>(entity.Id);

				NotifyCollectionChangedEventArgs collectionChangedArgs = null;
				loadedEntity.Items.CollectionChanged += (sender, args) => collectionChangedArgs = args;

				var newItem = new TestItem("Item 2", 20);
				loadedEntity.Items.Add(newItem);

				// Assert
				Assert.That(collectionChangedArgs, Is.Not.Null, "Событие CollectionChanged не было вызвано");
				Assert.That(collectionChangedArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
				Assert.That(collectionChangedArgs.NewItems[0], Is.EqualTo(newItem));
			}
		}

		[Test(Description = "Проверяем, что коллекция, загруженная через NHibernate, реагирует на удаление элементов")]
		public void LoadedCollection_RemoveItem_CollectionChangedEventFired() {
			NewSessionWithSameDB();
			// Должен оставаться открытым до завершения теста, чтобы база не удалилась из памяти
			using var uow = UnitOfWorkFactory.Create();

			// Arrange
			var entity = new EntityWithObservableList("Test Entity");
			entity.Items.Add(new TestItem("Item 1", 10));
			entity.Items.Add(new TestItem("Item 2", 20));

			uow.Save(entity);
			uow.Commit();

			using(var uow2 = UnitOfWorkFactory.Create()) {
				var loadedEntity = uow2.GetById<EntityWithObservableList>(entity.Id);
				var itemToRemove = loadedEntity.Items.First();

				NotifyCollectionChangedEventArgs collectionChangedArgs = null;
				loadedEntity.Items.CollectionChanged += (sender, args) => collectionChangedArgs = args;

				loadedEntity.Items.Remove(itemToRemove);

				// Assert
				Assert.That(collectionChangedArgs, Is.Not.Null, "Событие CollectionChanged не было вызвано");
				Assert.That(collectionChangedArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
				Assert.That(collectionChangedArgs.OldItems[0], Is.EqualTo(itemToRemove));
			}
		}

		[Test(Description =
			"Проверяем, что после удаления элемента из загруженной коллекции событие PropertyOfElementChanged больше не срабатывает")]
		public void LoadedCollection_RemoveItem_UnsubscribesFromElementPropertyChanged() {
			NewSessionWithSameDB();
			// Должен оставаться открытым до завершения теста, чтобы база не удалилась из памяти
			using var uow = UnitOfWorkFactory.Create();

			// Arrange
			var entity = new EntityWithObservableList("Test Entity");
			entity.Items.Add(new TestItem("Item 1", 10));

			uow.Save(entity);
			uow.Commit();

			// Act
			using(var uow2 = UnitOfWorkFactory.Create()) {
				var loadedEntity = uow2.GetById<EntityWithObservableList>(entity.Id);
				var item = loadedEntity.Items.First();

				int eventFiredCount = 0;
				loadedEntity.Items.PropertyOfElementChanged += (sender, args) => eventFiredCount++;

				loadedEntity.Items.Remove(item);
				item.Name = "Changed Name";

				// Assert
				Assert.That(eventFiredCount, Is.EqualTo(0),
					"Событие PropertyOfElementChanged не должно срабатывать после удаления элемента");
			}
		}

		[Test(Description = "Проверяем, что множественные изменения элементов в загруженной коллекции вызывают события")]
		public void LoadedCollection_MultipleElementChanges_AllEventsFired()
		{
			NewSessionWithSameDB();
			// Должен оставаться открытым до завершения теста, чтобы база не удалилась из памяти
			using var uow = UnitOfWorkFactory.Create();
			
			// Arrange
			var entity = new EntityWithObservableList("Test Entity");
			entity.Items.Add(new TestItem("Item 1", 10));
			entity.Items.Add(new TestItem("Item 2", 20));
			entity.Items.Add(new TestItem("Item 3", 30));
			
			uow.Save(entity);
			uow.Commit();
			
			// Act
			using (var uow2 = UnitOfWorkFactory.Create()) {
				var loadedEntity = uow2.GetById<EntityWithObservableList>(entity.Id);
				
				var changedProperties = new System.Collections.Generic.List<string>();
				loadedEntity.Items.PropertyOfElementChanged += (sender, args) => changedProperties.Add(args.PropertyName);
				
				// Изменяем свойства разных элементов
				loadedEntity.Items[0].Name = "Changed 1";
				loadedEntity.Items[1].Value = 200;
				loadedEntity.Items[2].Name = "Changed 3";
				loadedEntity.Items[2].Value = 300;
				
				// Assert
				Assert.That(changedProperties.Count, Is.EqualTo(4));
				Assert.That(changedProperties.Count(p => p == nameof(TestItem.Name)), Is.EqualTo(2));
				Assert.That(changedProperties.Count(p => p == nameof(TestItem.Value)), Is.EqualTo(2));
			}
		}

		[Test(Description = "Проверяем, что можем сохранить изменения в коллекции и элементах обратно в БД")]
		public void LoadedCollection_ModifyAndSave_ChangesPersisted()
		{
			NewSessionWithSameDB();
			// Должен оставаться открытым до завершения теста, чтобы база не удалилась из памяти
			using var uow = UnitOfWorkFactory.Create();
			// Arrange
			
			var entity = new EntityWithObservableList("Test Entity");
			entity.Items.Add(new TestItem("Item 1", 10));
			entity.Items.Add(new TestItem("Item 2", 20));

			uow.Save(entity);
			uow.Commit();

			// Act - изменяем и сохраняем
			using (var uow2 = UnitOfWorkFactory.Create()) {
				var loadedEntity = uow2.GetById<EntityWithObservableList>(entity.Id);
				
				loadedEntity.Items[0].Name = "Modified Item 1";
				loadedEntity.Items[0].Value = 100;
				loadedEntity.Items.Add(new TestItem("Item 3", 30));

				uow2.Save(loadedEntity);
				uow2.Commit();
			}

			// Assert - проверяем в новой сессии
			using (var uow3 = UnitOfWorkFactory.Create()) {
				var reloadedEntity = uow3.GetById<EntityWithObservableList>(entity.Id);
				
				Assert.That(reloadedEntity.Items.Count, Is.EqualTo(3));
				Assert.That(reloadedEntity.Items[0].Name, Is.EqualTo("Modified Item 1"));
				Assert.That(reloadedEntity.Items[0].Value, Is.EqualTo(100));
				Assert.That(reloadedEntity.Items[2].Name, Is.EqualTo("Item 3"));
			}
		}
	}
}

