using System;
using System.Linq;
using NUnit.Framework;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.DomainModel.NotifyChange;
using QS.Test.TestApp.Domain.Linked;
using QS.Testing.DB;

namespace QS.Test.Deletion
{
	[TestFixture()]
	public class DeleteIntegratedTest : InMemoryDBTestFixtureBase
	{

		[OneTimeSetUp]
		public void Init()
		{
			InitialiseNHibernate(typeof(RootDeleteItem).Assembly);
			NotifyConfiguration.Enable();
		}

		[Test(Description = "Проверяем что не пытаемся очищать ссылки в удаляемых объектах.")]
		[Category("real case")]
		public void Delete_NotFailWhenTryCleanPropertyOfDeletedInstanceTest()
		{
			//Тут очень важна последовательность конфига, окуратнее исправляйте тест.
			//Смысл теста в том, что через каскадное удаление объект в котором предполагалась очистка ссылки, удаляется перед, этим.
			// А в момент очистки ссылки он заново записывется, обычно с тем же ID, что в последствии вызывает различные проблемы,
			// например не возможно удалить объект, на который имеется ссылка из этого, или не удается этот записать, так как удален
			// тот на который мы ссылаемся. Повторюсь, мы не должны были его записывать.
			var delConfig = new DeleteConfiguration();
			delConfig.AddHibernateDeleteInfo<DependDeleteItem>()
				.AddDeleteCascadeDependence(x => x.CleanLink);
			delConfig.AddHibernateDeleteInfo<AlsoDeleteItem>()
				.AddClearDependence<DependDeleteItem>(x => x.CleanLink);
			delConfig.AddHibernateDeleteInfo<RootDeleteItem>()
				.AddDeleteDependence<DependDeleteItem>(x => x.DeleteLink)
				.AddDeleteDependence<AlsoDeleteItem>(x => x.Root);

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var root = new RootDeleteItem();
				var item1 = new AlsoDeleteItem() {
					Root = root
				};
				var item2 = new DependDeleteItem() {
					DeleteLink = root,
					CleanLink = item1
				};

				uow.Save(root);
				uow.Save(item1);
				uow.Save(item2);
				uow.Commit();

				var deletion = new DeleteCore(delConfig, uow);
				var cancellation = new System.Threading.CancellationToken();
				deletion.PrepareDeletion(typeof(RootDeleteItem), root.Id, cancellation);
				Assert.That(deletion.ItemsToDelete, Is.EqualTo(3));

				deletion.RunDeletion(cancellation);
				Assert.That(deletion.DeletionExecuted, Is.EqualTo(true));

				var dependDeleteItems = uow.GetAll<DependDeleteItem>().ToList();
				Assert.That(dependDeleteItems.Count, Is.EqualTo(0));
			}
		}
	}
}
