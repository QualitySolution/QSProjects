using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Navigation.GtkUI;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Test.GtkUI;
using QS.Test.TestApp.JournalViewModels;
using QS.Test.TestApp.ViewModels;
using QS.Test.TestApp.Views;
using QS.ViewModels;

namespace QS.Test.Navigation
{
	[TestFixture(TestOf = typeof(TdiNavigationManager))]
	public class TdiNavigationManagerTest
	{
		[Test(Description = "Проверка что действительно не открываем повторно вкладку с тем же хешем.")]
		public void OpenViewModel_DontCreateNewViewModel()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("hash_1");

			var commonService = Substitute.For<ICommonServices>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page = new Page<EntityViewModel>(viewModel, "hash_1");

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel).Returns(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1").ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory);

			var firstPage = navManager.OpenViewModel<EntityViewModel>(null);
			var secondPage = navManager.OpenViewModel<EntityViewModel>(null);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(1));
			Assert.That(firstPage, Is.EqualTo(secondPage));
		}

		[Test(Description = "Проверяем что страницы удаляются при закрытии.")]
		public void ForceClosePage_RemovePagesWhenClosedTest()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs((string)null);

			var commonService = Substitute.For<ICommonServices>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel1 = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page1 = new Page<EntityViewModel>(viewModel1, "page_1");
			var viewModel2 = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page2 = new Page<EntityViewModel>(viewModel2, "page_2");

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel1).ReturnsForAnyArgs(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, null).ReturnsForAnyArgs(page1, page2);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory);

			var firstPage = navManager.OpenViewModel<EntityViewModel, int>(null, 1);
			var secondPage = navManager.OpenViewModel<EntityViewModel, int>(null, 2);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(2));

			navManager.ForceClosePage(page1);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(1));
		}

		[Test(Description = "Проверка что событие о закрытии страницы приходит внешним подписчикам.")]
		public void Page_PageClosedEvent_RisedTest()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("hash_1");

			var commonService = Substitute.For<ICommonServices>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page = new Page<EntityViewModel>(viewModel, "hash_1");
			bool eventRised = false;
			page.PageClosed += (sender, e) => eventRised = true;

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel).Returns(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1").ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory);

			var firstPage = navManager.OpenViewModel<EntityViewModel>(null);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(1));

			navManager.ForceClosePage(firstPage);
			Assert.That(eventRised, Is.True);
		}

		[Test(Description = "Проверка что событие о закрытии страницы приходит для вкладки открытой в слайдере.")]
		public void Page_PageClosedEvent_RisedOnSlidedPageTest()
		{
			GtkInit.AtOnceInitGtk();
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("hash_1");

			var commonService = Substitute.For<ICommonServices>();
			var interactiveService = Substitute.For<IInteractiveService>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			IPage page = new Page<EntityViewModel>(viewModel, "hash_1");
			bool eventRised = false;
			page.PageClosed += (sender, e) => eventRised = true;

			var journalViewModel = Substitute.For<TabViewModelBase, ITdiJournal, ITdiTab>(interactiveService);
			IPage journal = new Page<TabViewModelBase>(journalViewModel, "journal_1");

			var tabJournalWidget = new ButtonSubscriptionView(viewModel);// Просто чтобы был хоть какой то настоящий виджет.
			var tabWidget = new ButtonSubscriptionView(viewModel);// Просто чтобы был хоть какой то настоящий виджет.
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(Arg.Any<TdiSliderTab>()).Returns(x => x[0]);
			resolver.Resolve(journalViewModel).Returns(tabJournalWidget);
			resolver.Resolve(viewModel).Returns(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<TabViewModelBase>(null, new System.Type[] { }, new object[] { }, "journal_1").ReturnsForAnyArgs(journal);
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1").ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory);

			var journalPage = navManager.OpenViewModel<TabViewModelBase>(null);

			var dialogPage = navManager.OpenViewModel<EntityViewModel>(journalPage.ViewModel);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(1));

			navManager.ForceClosePage(dialogPage);
			Assert.That(eventRised, Is.True);
		}
	}
}
