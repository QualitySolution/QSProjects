﻿using NUnit.Framework;
using QS.DomainModel.NotifyChange;
using QS.Test.TestApp.Domain;
using QS.Testing.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Test.DomainModel.NotifyChange
{
	[TestFixture()]
	[Category("Integrated")]
	public class NotifyChangeIntegratedTest : InMemoryDBTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			NotifyChangeEnable();
			InitialiseNHibernate(typeof(SimpleEntity).Assembly);
		}

		class BatchEventTestSubscriber : IDisposable
		{
			private readonly AppLevelChangeListener changeWatcher;
			public List<EntityChangeEvent[]> calls = new List<EntityChangeEvent[]>();
			
			public BatchEventTestSubscriber(AppLevelChangeListener changeWatcher) {
				this.changeWatcher = changeWatcher;
			}

			public void Dispose()
			{
				changeWatcher.UnsubscribeAll(this);
			}

			public void Handler(EntityChangeEvent[] changeEvents)
			{
				calls.Add(changeEvents);
			}
		}

		#region Проверка условий

		[Test(Description = "Проверяем что реагируем только на определенный тип событий, в частности удаление.")]
		public void NotifyOnlyDeleteEventTest()
		{
			using (var subscriber = new BatchEventTestSubscriber(ChangeWatcher)) {
				ChangeWatcher.BatchSubscribe(subscriber.Handler)
					.IfEntity<SimpleEntity>().AndChangeType(TypeOfChangeEvent.Delete);

				using (var uow = UnitOfWorkFactory.Create()) {
					var entity = new SimpleEntity();
					uow.Save(entity);
					uow.Commit();
					uow.Session.Evict(entity);

					var loadedEntity = uow.GetById<SimpleEntity>(1);
					uow.Delete(loadedEntity);
					uow.Commit();
				}

				Assert.That(subscriber.calls.Count, Is.EqualTo(1));
				var changeEvents = subscriber.calls.First();
				Assert.That(changeEvents.First().EventType, Is.EqualTo(TypeOfChangeEvent.Delete));
			}
		}

		[Test(Description = "Проверяем что можем фильтровать события по свойству объекта.")]
		public void NotifyByWhereConditionTest()
		{
			using (var subscriber = new BatchEventTestSubscriber(ChangeWatcher)) {
				ChangeWatcher.BatchSubscribe(subscriber.Handler)
					.IfEntity<SimpleEntity>().AndWhere(x => x.Text == "1");

				using (var uow = UnitOfWorkFactory.Create()) {
					var entity1 = new SimpleEntity { Text = "1" };
					uow.Save(entity1);

					var entity2 = new SimpleEntity { Text = "2" };
					uow.Save(entity2);
					uow.Commit();
				}

				Assert.That(subscriber.calls.Count, Is.EqualTo(1));
				var changeEvents = subscriber.calls.First();
				Assert.That(changeEvents.Count(), Is.EqualTo(1));
				Assert.That(changeEvents.First().EventType, Is.EqualTo(TypeOfChangeEvent.Insert));
				var entity = changeEvents.First().GetEntity<SimpleEntity>();
				Assert.That(entity.Text, Is.EqualTo("1"));
			}
		}

		[Test(Description = "Проверяем что можем фильтровать события с двумя условиями через И.")]
		public void NotifyByWhereAndTwoConditionTest()
		{
			using (var subscriber = new BatchEventTestSubscriber(ChangeWatcher)) {
				ChangeWatcher.BatchSubscribe(subscriber.Handler)
					.IfEntity<SimpleEntity>()
					.AndWhere(x => x.Text.Contains("1"))
					.AndWhere(x => x.Text.Contains("2"));

				using (var uow = UnitOfWorkFactory.Create()) {
					var entity1 = new SimpleEntity { Text = "1,2" };
					uow.Save(entity1);
					var entity2 = new SimpleEntity { Text = "2" };
					uow.Save(entity2);
					var entity3 = new SimpleEntity { Text = "1" };
					uow.Save(entity3);
					uow.Commit();
				}

				Assert.That(subscriber.calls.Count, Is.EqualTo(1));
				var changeEvents = subscriber.calls.First();
				Assert.That(changeEvents.Count(), Is.EqualTo(1));
				Assert.That(changeEvents.First().EventType, Is.EqualTo(TypeOfChangeEvent.Insert));
				var entity = changeEvents.First().GetEntity<SimpleEntity>();
				Assert.That(entity.Text, Is.EqualTo("1,2"));
			}
		}

		[Test(Description = "Проверяем что можем фильтровать события с двумя условиями через ИЛИ.")]
		public void NotifyByWhereOrTwoConditionTest()
		{
			using (var subscriber = new BatchEventTestSubscriber(ChangeWatcher)) {
				ChangeWatcher.BatchSubscribe(subscriber.Handler)
					.IfEntity<SimpleEntity>()
					.AndWhere(x => x.Text.Contains("1"))
					.Or.IfEntity<SimpleEntity>()
					.AndWhere(x => x.Text.Contains("2"));

				using (var uow = UnitOfWorkFactory.Create()) {
					var entity1 = new SimpleEntity { Text = "1,2" };
					uow.Save(entity1);
					var entity2 = new SimpleEntity { Text = "2" };
					uow.Save(entity2);
					var entity3 = new SimpleEntity { Text = "1" };
					uow.Save(entity3);
					var entity4 = new SimpleEntity { Text = "4" };
					uow.Save(entity4);
					uow.Commit();
				}

				Assert.That(subscriber.calls.Count, Is.EqualTo(1));
				var changeEvents = subscriber.calls.First();
				Assert.That(changeEvents.Count(), Is.EqualTo(3));
			}
		}

		#endregion

		[Test(Description = "Проверяем что подписка BatchSubscribeOnEntity реагирует на сохранение.")]
		public void DirectBatchSubscribe_NotifySaveEventTest()
		{
			using (var subscriber = new BatchEventTestSubscriber(ChangeWatcher)) {

				ChangeWatcher.BatchSubscribeOnEntity<SimpleEntity>(subscriber.Handler);

				using (var uow = UnitOfWorkFactory.Create()) {
					var entity = new SimpleEntity();
					uow.Save(entity);
					uow.Commit();
				}
				Assert.That(subscriber.calls.Count, Is.EqualTo(1));
			}
		}

		[Test(Description = "Проверяем что можем получить значения полей.")]
		public void BatchSubscribe_InsertAndUpdateAndDelete_GetOldAndNewPropertyValuesTest()
		{
			using (var subscriber = new BatchEventTestSubscriber(ChangeWatcher)) {

				ChangeWatcher.BatchSubscribeOnEntity<SimpleEntity>(subscriber.Handler);

				using (var uow = UnitOfWorkFactory.Create()) {
					var entity = new SimpleEntity();
					entity.Text = "Test text";
					uow.Save(entity);
					uow.Commit();
					uow.Session.Evict(entity);

					var loadedEntity = uow.GetById<SimpleEntity>(1);
					loadedEntity.Text = "New test text";
					uow.Save(loadedEntity);
					uow.Commit();

					uow.Delete(loadedEntity);
					uow.Commit();
				}
				Assert.That(subscriber.calls.Count, Is.EqualTo(3));

				var event1 = subscriber.calls[0].First();
				Assert.That(event1.GetOldValue<SimpleEntity>(x => x.Text), Is.Null);
				Assert.That(event1.GetNewValue<SimpleEntity>(x => x.Text), Is.EqualTo("Test text"));

				var event2 = subscriber.calls[1].First();
				Assert.That(event2.GetOldValue<SimpleEntity>(x => x.Text), Is.EqualTo("Test text"));
				Assert.That(event2.GetNewValue<SimpleEntity>(x => x.Text), Is.EqualTo("New test text"));

				var event3 = subscriber.calls[2].First();
				Assert.That(event3.GetOldValue<SimpleEntity>(x => x.Text), Is.EqualTo("New test text"));
				Assert.That(event3.GetNewValue<SimpleEntity>(x => x.Text), Is.Null);
			}
		}
	}
}
