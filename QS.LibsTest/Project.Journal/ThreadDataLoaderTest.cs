using System;
using System.Collections.Generic;
using System.Threading;
using NHibernate;
using NHibernate.Transform;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Test.TestApp.Domain;
using QS.Test.TestApp.JournalViewModels;
using QS.Testing.DB;

namespace QS.Test.Project.Journal
{
	[TestFixture()]
	public class ThreadDataLoaderTest : InMemoryDBTestFixtureBase
	{
		IQueryOver<Document1> GetDocument1Query(IUnitOfWork u)
		{
			DocumentJournalNode resultAlias = null;
			Document1 Document1Alias = null;

			return u.Session.QueryOver<Document1>(() => Document1Alias)
				.SelectList(list => list
			.Select(() => Document1Alias.Id).WithAlias(() => resultAlias.Id)
			.Select(() => Document1Alias.Date).WithAlias(() => resultAlias.Date)
			)
			.OrderBy(x => x.Date).Desc
			.TransformUsing(Transformers.AliasToBean<DocumentJournalNode<Document1>>());
		}

		IQueryOver<Document2> GetDocument2Query(IUnitOfWork u)
		{
			DocumentJournalNode resultAlias = null;
			Document2 Document2Alias = null;

			return u.Session.QueryOver<Document2>(() => Document2Alias)
				.SelectList(list => list
			.Select(() => Document2Alias.Id).WithAlias(() => resultAlias.Id)
			.Select(() => Document2Alias.Date).WithAlias(() => resultAlias.Date)
			)
			.OrderBy(x => x.Date).Desc
			.TransformUsing(Transformers.AliasToBean<DocumentJournalNode<Document2>>());
		}

		[Test(Description = "Проверяем что действительно корекнто объединяем 2 запроса. В нужном порядке.")]
		public void CorrectUnionTwoQueryCase()
		{
			InitialiseNHibernate(typeof(Document1).Assembly);
			NewSessionWithSameDB();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var listToSave = new List<IDomainObject> {
					new Document1(new DateTime(2019, 1, 1)),
					new Document1(new DateTime(2019, 1, 1)),
					new Document1(new DateTime(2019, 1, 1)),
					new Document1(new DateTime(2019, 1, 1)),
					new Document2(new DateTime(2018, 1, 1)),
					new Document2(new DateTime(2017, 1, 1)),
					new Document1(new DateTime(2016, 1, 1)),
					new Document2(new DateTime(2017, 5, 23)),
				 };

				foreach (var doc in listToSave) {
					uow.Save(doc);
				}
				uow.Commit();


				ManualResetEvent oSignalEvent = new ManualResetEvent(false);
				Exception treadException = null;
				bool itemsListUpdatedRised = false;

				//Настраиваем загрузчик
				var dataLoader = new ThreadDataLoader<DocumentJournalNode>(UnitOfWorkFactory);
				dataLoader.PageSize = 2;

				dataLoader.AddQuery(GetDocument1Query);

				dataLoader.AddQuery(GetDocument2Query);

				dataLoader.MergeInOrderBy((doc) => doc.Date, true);

				dataLoader.LoadError += (sender, e) => {
					Console.WriteLine("LoadError");
					treadException = e.Exception;
					oSignalEvent.Set();
				};
				dataLoader.ItemsListUpdated += (sender, e) => {
					Console.WriteLine("ItemsListUpdated");
					itemsListUpdatedRised = true;
				};
				dataLoader.LoadingStateChanged += (sender, e) => {
					Console.WriteLine($"LoadingStateChanged {e.LoadingState}");
					if (e.LoadingState == LoadingState.Idle)
						oSignalEvent.Set();
				};

				//Загружаем данные
				dataLoader.LoadData(false);
				Assert.That(dataLoader.LoadInProgress, Is.True);
				oSignalEvent.WaitOne();
				oSignalEvent.Reset();
				if (treadException != null)
					throw treadException;
				Assert.That(itemsListUpdatedRised, Is.EqualTo(true));
				//Предпологаем что загрузили всего 2 строки, в журнале у нас размер страницы 2.
				var result = (IList<DocumentJournalNode>)dataLoader.Items;
				Assert.That(result.Count, Is.GreaterThanOrEqualTo(2));
				Assert.That(result[0].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[1].Date, Is.EqualTo(new DateTime(2019, 1, 1)));

				//Загружаем следующую партию.
				itemsListUpdatedRised = false;
				dataLoader.LoadData(true);
				oSignalEvent.WaitOne();
				oSignalEvent.Reset();
				//Предпологаем что загрузили еще минимум 2 строки.
				Assert.That(itemsListUpdatedRised, Is.EqualTo(true));
				result = (IList<DocumentJournalNode>)dataLoader.Items;
				Assert.That(result.Count, Is.GreaterThanOrEqualTo(4));
				Assert.That(result[0].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[1].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[2].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[3].Date, Is.EqualTo(new DateTime(2019, 1, 1)));

				//Загружаем следующую партию.
				itemsListUpdatedRised = false;
				dataLoader.LoadData(true);
				oSignalEvent.WaitOne();
				oSignalEvent.Reset();

				Assert.That(itemsListUpdatedRised, Is.EqualTo(true));
				result = (IList<DocumentJournalNode>)dataLoader.Items;
				Assert.That(result.Count, Is.GreaterThanOrEqualTo(6));
				Assert.That(result[0].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[1].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[2].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[3].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[4].Date, Is.EqualTo(new DateTime(2018, 1, 1)));
				Assert.That(result[5].Date, Is.EqualTo(new DateTime(2017, 5, 23)));
			}
		}

		[Test(Description = "Проверяем что не грузим документы к которым у нас нет доступа на чтение.")]
		public void AcssesPermissionTestCase()
		{
			InitialiseNHibernate(typeof(Document1).Assembly);
			NewSessionWithSameDB();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var listToSave = new List<IDomainObject> {
					new Document1(new DateTime(2019, 1, 1)),
					new Document2(new DateTime(2017, 5, 23)),
					new Document1(new DateTime(2019, 1, 1)),
				 };

				foreach (var doc in listToSave) {
					uow.Save(doc);
				}
				uow.Commit();

				ManualResetEvent oSignalEvent = new ManualResetEvent(false);
				Exception treadException = null;
				bool itemsListUpdatedRised = false;

				var deniedRead = Substitute.For<IPermissionResult>();
				deniedRead.CanRead.Returns(false);

				var allowedRead = Substitute.For<IPermissionResult>();
				allowedRead.CanRead.Returns(true);

				var permissionService = Substitute.For<ICurrentPermissionService>();
				permissionService.ValidateEntityPermission(typeof(Document1)).Returns(deniedRead);
				permissionService.ValidateEntityPermission(typeof(Document2)).Returns(allowedRead);

				//Настраиваем загрузчик
				var dataLoader = new ThreadDataLoader<DocumentJournalNode>(UnitOfWorkFactory);
				dataLoader.CurrentPermissionService = permissionService;

				dataLoader.AddQuery(GetDocument1Query);

				dataLoader.AddQuery(GetDocument2Query);

				dataLoader.MergeInOrderBy((doc) => doc.Date, true);

				dataLoader.LoadError += (sender, e) => {
					treadException = e.Exception;
					oSignalEvent.Set();
				};
				dataLoader.ItemsListUpdated += (sender, e) => {
					itemsListUpdatedRised = true;
				};
				dataLoader.LoadingStateChanged += (sender, e) => {
					if (e.LoadingState == LoadingState.Idle)
						oSignalEvent.Set();
				};

				//Загружаем данные
				dataLoader.LoadData(false);
				Assert.That(dataLoader.LoadInProgress, Is.True);
				oSignalEvent.WaitOne();
				oSignalEvent.Reset();
				if (treadException != null)
					throw treadException;

				Assert.That(itemsListUpdatedRised, Is.EqualTo(true));
				var result = (IList<DocumentJournalNode>)dataLoader.Items;
				Assert.That(result[0].Date, Is.EqualTo(new DateTime(2017, 5, 23)));
				Assert.That(result.Count, Is.EqualTo(1));
			}
		}
	}
}
