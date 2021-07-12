using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Test.TestApp.Domain;
using QS.Test.TestApp.JournalViewModels;
using QS.Testing.DB;

namespace QS.Test.Project.Journal
{
	[TestFixture()]
	public class EntityJournalViewModelBaseTest : InMemoryDBTestFixtureBase
	{
		[Test(Description = "Тест корректного получения данных из журнала при установке запроса целиком.")]
		public void ItemsQuery_FullSetTest()
		{
			InitialiseNHibernate(typeof(Document1).Assembly);
			NewSessionWithSameDB();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var listToSave = new List<Document1> {
					new Document1(new DateTime(2019, 1, 1)),
					new Document1(new DateTime(2019, 2, 2)),
					new Document1(new DateTime(2019, 3, 3)),
				 };

				foreach (var doc in listToSave) {
					uow.Save(doc);
				}
				uow.Commit();

				var interactiveService = Substitute.For<IInteractiveService>();
				var navigationManager = Substitute.For<INavigationManager>();

				var model = new FullQuerySetEntityJournalViewModel(UnitOfWorkFactory, interactiveService, navigationManager);

				ManualResetEvent oSignalEvent = new ManualResetEvent(false);
				Exception treadException = null;
				bool itemsListUpdatedRised = false;

				model.DataLoader.LoadError += (sender, e) => {
					treadException = e.Exception;
					oSignalEvent.Set();
				};
				model.DataLoader.ItemsListUpdated += (sender, e) => {
					itemsListUpdatedRised = true;
				};
				model.DataLoader.LoadingStateChanged += (sender, e) => {
					if (e.LoadingState == LoadingState.Idle)
						oSignalEvent.Set();
				};

				model.Refresh();
				oSignalEvent.WaitOne();
				if (treadException != null)
					throw treadException;
				Assert.That(itemsListUpdatedRised, Is.EqualTo(true));
				var result = (IList<FullQuerySetDocumentJournalNode>)model.Items;
				Assert.That(result.Count, Is.EqualTo(3));
				Assert.That(result[0].Date, Is.EqualTo(new DateTime(2019, 1, 1)));
				Assert.That(result[1].Date, Is.EqualTo(new DateTime(2019, 2, 2)));
				Assert.That(result[2].Date, Is.EqualTo(new DateTime(2019, 3, 3)));

			}
		}
	}
}
