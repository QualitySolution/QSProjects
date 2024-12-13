﻿using System.Linq;
using NUnit.Framework;
using QS.Deletion;
using QS.Deletion.Configuration;
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
		}

		[Test(Description = "Проверяем что не пытаемся очищать ссылки в удаляемых объектах.")]
		[Category("real case")]
		public void Delete_NotFailWhenTryCleanPropertyOfDeletedInstanceTest()
		{
			// Тут очень важна последовательность конфига, аккуратнее исправляйте тест.
			// Смысл теста в том, что через каскадное удаление объект в котором предполагалась очистка ссылки, удаляется перед.
			// А в момент очистки ссылки он заново записывается в базу, обычно с тем же ID, что в последствии вызывает различные проблемы,
			// например не возможно удалить объект, на который уже якобы удаленный объект ссылается или не удается записать левый объект, так как удален
			// тот на который мы ссылаемся. Повторюсь, мы не должны были его записывать.
			var delConfig = new DeleteConfiguration(NhConfiguration);
			delConfig.AddHibernateDeleteInfo<DependDeleteItem>()
				.AddDeleteCascadeDependence(x => x.CleanLink);
			delConfig.AddHibernateDeleteInfo<AlsoDeleteItem>()
				.AddClearDependence<DependDeleteItem>(x => x.CleanLink);
			delConfig.AddHibernateDeleteInfo<RootDeleteItem>()
				.AddDeleteDependence<DependDeleteItem>(x => x.DeleteLink)
				.AddDeleteDependence<AlsoDeleteItem>(x => x.Root);

			using (var uow = UnitOfWorkFactory.Create()) {
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

				var deletion = new DeleteCore(delConfig, uow: uow);
				var cancellation = new System.Threading.CancellationToken();
				deletion.PrepareDeletion(typeof(RootDeleteItem), root.Id, cancellation);
				Assert.That(deletion.ItemsToDelete, Is.EqualTo(3));

				deletion.RunDeletion(cancellation);
				Assert.That(deletion.DeletionExecuted, Is.EqualTo(true));
				uow.Commit();

				var dependDeleteItems = uow.GetAll<DependDeleteItem>().ToList();
				Assert.That(dependDeleteItems.Count, Is.EqualTo(0));
			}
		}
	}
}
