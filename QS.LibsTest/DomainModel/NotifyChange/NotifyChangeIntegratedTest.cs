using NUnit.Framework;
using QS.DB;
using QS.DomainModel.NotifyChange;
using QS.Test.TestDomain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Test.DomainModel.NotifyChange
{
	[TestFixture()]
	public class NotifyChangeIntegratedTest : InMemoryDBTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			InitialiseNHibernate(typeof(SimpleEntity).Assembly);
			NotifyConfiguration.Enable();
		}

		class BatchEventTestSubscruber : IDisposable
		{
			public List<EntityChangeEvent[]> calls = new List<EntityChangeEvent[]>();

			public void Dispose()
			{
				NotifyConfiguration.Instance.UnsubscribeAll(this);
			}

			public void Handler(EntityChangeEvent[] changeEvents)
			{
				calls.Add(changeEvents);
			}
		}

		[Test(Description = "Проверяем что реагируем только на определенный тип событий, в частности удаление.")]
		public void NotifyOnlyDeleteEventTest()
		{
			using (var subscruber = new BatchEventTestSubscruber()) {
				NotifyConfiguration.Instance.BatchSubscribe(subscruber.Handler)
					.IfEntity<SimpleEntity>().AndChangeType(TypeOfChangeEvent.Delete);

				using (var uow = UnitOfWorkFactory.CreateWithNewRoot<SimpleEntity>()) {
					uow.Save();
					uow.Session.Evict(uow.Root);

					var loadedEntity = uow.GetById<SimpleEntity>(1);
					uow.Delete(loadedEntity);
					uow.Commit();
				}

				Assert.That(subscruber.calls.Count, Is.EqualTo(1));
				var changeEvents = subscruber.calls.First();
				Assert.That(changeEvents.First().EventType, Is.EqualTo(TypeOfChangeEvent.Delete));
			}
		}

		[Test(Description = "Проверяем что реагируем только на определенный тип событий, в частности удаление.")]
		public void DirectBatchSubscribe_NotifySaveEventTest()
		{
			using (var subscruber = new BatchEventTestSubscruber()) {

				NotifyConfiguration.Instance.BatchSubscribeOnEntity<SimpleEntity>(subscruber.Handler);

				using (var uow = UnitOfWorkFactory.CreateWithNewRoot<SimpleEntity>()) {
					uow.Save();
				}
				Assert.That(subscruber.calls.Count, Is.EqualTo(1));
			}
		}

	}


}
