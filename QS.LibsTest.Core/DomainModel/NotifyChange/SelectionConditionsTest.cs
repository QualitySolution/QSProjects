using NHibernate.Event;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.NotifyChange.Conditions;
using QS.DomainModel.UoW;
using QS.Test.TestApp.Domain;

namespace QS.Test.DomainModel.NotifyChange {
	[TestFixture(TestOf = typeof(SelectionConditions))]
	public class SelectionConditionsTest {
		
		[Test(Description = "Проверяем что можем игнорировать события из указанных сессий")]
		public void ExcludeUowTest()
		{
			var conditions = new SelectionConditions();
			var uow1 = Substitute.For<IUnitOfWork>();
			var session1 = Substitute.For<IEventSource>();
			uow1.Session.Returns(session1);
			var uow2 = Substitute.For<IUnitOfWork>();
			var session2 = Substitute.For<IEventSource>();
			uow2.Session.Returns(session2);
			var uow3 = Substitute.For<IUnitOfWork>();
			var session3 = Substitute.For<IEventSource>();
			uow3.Session.Returns(session3);
			
			conditions.ExcludeUow(uow1, uow2);
			
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Insert, session: session1)), Is.False);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Update, session: session2)), Is.False);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Delete, session: session3)), Is.True);
		}
		
		[Test(Description = "Проверяем что можем подписаться только на указанные сессий")]
		public void OnlyForUowTest()
		{
			var conditions = new SelectionConditions();
			var uow1 = Substitute.For<IUnitOfWork>();
			var session1 = Substitute.For<IEventSource>();
			uow1.Session.Returns(session1);
			var uow2 = Substitute.For<IUnitOfWork>();
			var session2 = Substitute.For<IEventSource>();
			uow2.Session.Returns(session2);
			var uow3 = Substitute.For<IUnitOfWork>();
			var session3 = Substitute.For<IEventSource>();
			uow3.Session.Returns(session3);
			
			conditions.OnlyForUow(uow1, uow3);
			
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Insert, session: session1)), Is.True);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Update, session: session2)), Is.False);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Delete, session: session3)), Is.True);
		}
		
		[Test(Description = "Проверяем что можем подписаться на события нескольких типов одновременно.")]
		public void AndChangeTypeTest()
		{
			var conditions = new SelectionConditions();
			
			conditions
				.IfEntity<SimpleEntity>()
				.AndChangeType(TypeOfChangeEvent.Insert)
				.AndChangeType(TypeOfChangeEvent.Delete);
			
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Insert, entityClass: typeof(SimpleEntity))), Is.True);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Update, entityClass: typeof(SimpleEntity))), Is.False);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Delete, entityClass: typeof(SimpleEntity))), Is.True);
		}
		
		[Test(Description = "Проверяем что можем подписаться на события конкретного класса.")]
		public void IfEntityTest()
		{
			var conditions = new SelectionConditions();
			
			conditions
				.IfEntity<SimpleEntity>();
			
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Insert, entityClass: typeof(SimpleEntity))), Is.True);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Update, entityClass: typeof(SimpleEntity))), Is.True);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Delete, entityClass: typeof(SimpleEntity))), Is.True);
			
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Insert, entityClass: typeof(ValidatedEntity))), Is.False);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Update, entityClass: typeof(ValidatedEntity))), Is.False);
			Assert.That(conditions.IsSuitable(new EntityChangeEvent(TypeOfChangeEvent.Delete, entityClass: typeof(ValidatedEntity))), Is.False);
		}
	}
}
