using NUnit.Framework;
using QS.DB;
using QS.Test.TestDomain;

namespace QS.Test.DomainModel.Entity
{
	[TestFixture()]
	public class BusinessObjectIntegratedTests : InMemoryDBTestFixtureBase
	{
		[Test(Description = "Проверяем что механизм заполения поля Uow при загрузке объекта с интерфейсом IBusinessObject, в принцепе работает.")]
		public void IBusinessObjectFillUowPropertyTest()
		{
			InitialiseNHibernate(typeof(BusinessObjectTestEntity).Assembly);

			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<BusinessObjectTestEntity>()) {
				uow.Save();
				var savedEntity = uow.Root;
				uow.Session.Evict(savedEntity);

				var loadedEntity = uow.GetById<BusinessObjectTestEntity>(1);
				Assert.That(loadedEntity, Is.Not.EqualTo(savedEntity));
				Assert.That(loadedEntity.UoW, Is.EqualTo(uow));
			}
		}
	}
}