using System.Collections.Generic;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Test.GtkUI;
using QS.Test.TestApp.Dialogs;
using QS.Test.TestApp.ViewModels;
using QS.Test.TestApp.Views;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using QS.Views.Resolve;

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
			var interactiveMessage = Substitute.For<IInteractiveMessage>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page = new TdiPage<EntityViewModel>(viewModel, viewModel, "hash_1");

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel).Returns(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1", null).ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveMessage);

			var firstPage = navManager.OpenViewModel<EntityViewModel>(null);
			var secondPage = navManager.OpenViewModel<EntityViewModel>(null);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(1));
			Assert.That(firstPage, Is.EqualTo(secondPage));
		}

		[Test(Description = "[НА ЭТО ПОВЕДЕНИЕ рассчитывает JournalViewModelSelector] Проверка что действительно не открываем повторно вкладку с тем же хешем для подчиненой вкладке с той же главной.")]
		public void OpenViewModel_DontCreateNewViewModelAsSlave()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("master_1", "hash_1", "hash_1");

			var commonService = Substitute.For<ICommonServices>();
			var interactiveMessage = Substitute.For<IInteractiveMessage>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var masterViewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page = new TdiPage<EntityViewModel>(viewModel, viewModel, "hash_1");
			var masterPage = new TdiPage<EntityViewModel>(masterViewModel, masterViewModel, "master_1");

			var tabWidget = Substitute.For<Gtk.Widget>();
			var masterTabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel).Returns(tabWidget);
			resolver.Resolve((ITdiTab)masterViewModel).Returns(masterTabWidget);
			var tdiNotebook = new TdiNotebook();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "_1", null).ReturnsForAnyArgs(masterPage, page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveMessage);

			var zeroPage = navManager.OpenViewModel<EntityViewModel>(null);
			Assert.That(zeroPage, Is.EqualTo(masterPage));

			var firstPage = navManager.OpenViewModel<EntityViewModel>(masterViewModel, OpenPageOptions.AsSlave);
			var secondPage = navManager.OpenViewModel<EntityViewModel>(masterViewModel, OpenPageOptions.AsSlave);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(2));
			Assert.That(firstPage, Is.EqualTo(secondPage));
		}

		[Test(Description = "[НА ЭТО ПОВЕДЕНИЕ рассчитывает JournalViewModelSelector] Проверка что действительно не открываем повторно вкладку с тем же хешем для подчиненой вкладке с той же главной TdiTab в режиме совместимости.")]
		public void OpenViewModel_DontCreateNewViewModelAsSlave_TdiMixedMaster()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("hash_1");

			var commonService = Substitute.For<ICommonServices>();
			var interactiveMessage = Substitute.For<IInteractiveMessage>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var masterTab = Substitute.For<ITdiTab, Gtk.Widget>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page = new TdiPage<EntityViewModel>(viewModel, viewModel, "hash_1");

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel).Returns(tabWidget);
			resolver.Resolve(masterTab).Returns((Gtk.Widget)masterTab);
			var tdiNotebook = new TdiNotebook();
			tdiNotebook.WidgetResolver = resolver;
			tdiNotebook.AddTab(masterTab);

			Assert.That(tdiNotebook.Tabs.Count, Is.EqualTo(1));

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1", null).ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveMessage);

			var firstPage = navManager.OpenViewModelOnTdi<EntityViewModel>(masterTab, OpenPageOptions.AsSlave);
			var secondPage = navManager.OpenViewModelOnTdi<EntityViewModel>(masterTab, OpenPageOptions.AsSlave);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(2));
			Assert.That(firstPage, Is.EqualTo(secondPage));
		}

		[Test(Description = "Проверяем что страницы удаляются при закрытии.")]
		public void ForceClosePage_RemovePagesWhenClosedTest()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs((string)null);

			var commonService = Substitute.For<ICommonServices>();
			var interactiveMessage = Substitute.For<IInteractiveMessage>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel1 = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page1 = new TdiPage<EntityViewModel>(viewModel1, viewModel1, "page_1");
			var viewModel2 = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page2 = new TdiPage<EntityViewModel>(viewModel2, viewModel2, "page_2");

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel1).ReturnsForAnyArgs(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, null, null).ReturnsForAnyArgs(page1, page2);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveMessage);

			var firstPage = navManager.OpenViewModel<EntityViewModel, int>(null, 1);
			var secondPage = navManager.OpenViewModel<EntityViewModel, int>(null, 2);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(2));

			navManager.ForceClosePage(page1);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(1));
		}

		[Test(Description = "Проверяем что страницы c чистым TDi открытые в режиме совместимости тоже удаляются.")]
		public void Tdinotebook_RemoveTDiPagesWhenClosedTest()
		{
			GtkInit.AtOnceInitGtk();
			var hashGenerator = new ClassNamesHashGenerator(null);

			var commonService = Substitute.For<ICommonServices>();
			var interactiveMessage = Substitute.For<IInteractiveMessage>();
			var interactiveService = Substitute.For<IInteractiveService>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			var tabPageFactory = Substitute.For<ITdiPageFactory>();

			var tab = new EmptyDlg();
			var tabpage = new TdiTabPage(tab, null);

			tabPageFactory.CreateTdiPageTypedArgs<EmptyDlg>(new System.Type[] { }, new object[] { }, "hash_1").ReturnsForAnyArgs(tabpage);

			var notebook = new TdiNotebook();
			var navManager = new TdiNavigationManager(notebook, hashGenerator, pageFactory, interactiveMessage, tabPageFactory);

			var page = navManager.OpenTdiTab<EmptyDlg>(null);

			Assert.That(navManager.TopLevelPages.Count, Is.EqualTo(1));

			notebook.ForceCloseTab(page.TdiTab);

			Assert.That(navManager.TopLevelPages.Count, Is.EqualTo(0));
		}

		[Test(Description = "Проверка что событие о закрытии страницы приходит внешним подписчикам.")]
		public void Page_PageClosedEvent_RisedTest()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("hash_1");

			var commonService = Substitute.For<ICommonServices>();
			var interactiveMessage = Substitute.For<IInteractiveMessage>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page = new TdiPage<EntityViewModel>(viewModel, viewModel, "hash_1");
			bool eventRised = false;
			page.PageClosed += (sender, e) => eventRised = true;

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel).Returns(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1", null).ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveMessage);

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
			var navigation = Substitute.For<INavigationManager>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			IPage page = new TdiPage<EntityViewModel>(viewModel, viewModel, "hash_1");
			bool eventRised = false;
			page.PageClosed += (sender, e) => eventRised = true;

			var journalViewModel = Substitute.For<TabViewModelBase, ITdiJournal, ITdiTab>(interactiveService, navigation);
			IPage journal = new TdiPage<TabViewModelBase>(journalViewModel, journalViewModel, "journal_1");

			var tabJournalWidget = new ButtonSubscriptionView(viewModel);// Просто чтобы был хоть какой то настоящий виджет.
			var tabWidget = new ButtonSubscriptionView(viewModel);// Просто чтобы был хоть какой то настоящий виджет.
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(Arg.Any<TdiSliderTab>()).Returns(x => x[0]);
			resolver.Resolve(journalViewModel).Returns(tabJournalWidget);
			resolver.Resolve(viewModel).Returns(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<TabViewModelBase>(null, new System.Type[] { }, new object[] { }, "journal_1", null).ReturnsForAnyArgs(journal);
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1", null).ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveService);

			var journalPage = navManager.OpenViewModel<TabViewModelBase>(null);

			var dialogPage = navManager.OpenViewModel<EntityViewModel>(journalPage.ViewModel);

			Assert.That(navManager.TopLevelPages.Count(), Is.EqualTo(1));

			navManager.ForceClosePage(dialogPage);
			Assert.That(eventRised, Is.True);
		}

		[Test(Description = "Проверка что событие о закрытии страницы приходит для вкладки открытой в слайдере, при закрытии слайдера из tdi. " +
			"Реальный баг, навигатор не находит страцицу если в событии о закрытии вкладки прилетал слайдер, а не вкладка журнала.")]
		public void Page_PageClosedEvent_WhenTdiCloseSliderTest()
		{
			GtkInit.AtOnceInitGtk();
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("journal_1");

			var commonService = Substitute.For<ICommonServices>();
			var interactiveService = Substitute.For<IInteractiveService>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var navigation = Substitute.For<INavigationManager>();

			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);

			var journalViewModel = Substitute.For<TabViewModelBase, ITdiJournal, ITdiTab>(interactiveService, navigation);
			bool eventRised = false;
			IPage journal = new TdiPage<TabViewModelBase>(journalViewModel, journalViewModel, "journal_1");
			journal.PageClosed += (sender, e) => eventRised = true;

			var tabJournalWidget = new ButtonSubscriptionView(viewModel);// Просто чтобы был хоть какой то настоящий виджет.
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(Arg.Any<TdiSliderTab>()).Returns(x => x[0]);
			resolver.Resolve(journalViewModel).Returns(tabJournalWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<TabViewModelBase>(null, new System.Type[] { }, new object[] { }, "journal_1", null).ReturnsForAnyArgs(journal);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveService);

			var journalPage = navManager.OpenViewModel<TabViewModelBase>(null);

			Assert.That(tdiNotebook.Tabs.Count, Is.EqualTo(1));
			var openedTab = tdiNotebook.Tabs.First().TdiTab;
			tdiNotebook.ForceCloseTab(openedTab);

			Assert.That(eventRised, Is.True);
			Assert.That(navManager.TopLevelPages.Count, Is.EqualTo(0));
		}

		[Test(Description = "Проверка что можем найти страницу передва DialogViewModelBase.(Реальный баг)")]
		public void FindPage_ByDialogViewModelBaseTest()
		{
			var hashGenerator = Substitute.For<IPageHashGenerator>();
			hashGenerator.GetHash<EntityViewModel>(null, new System.Type[] { }, new object[] { }).ReturnsForAnyArgs("hash_1");

			var commonService = Substitute.For<ICommonServices>();
			var interactiveMessage = Substitute.For<IInteractiveMessage>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();
			var viewModel = Substitute.For<EntityViewModel, ITdiTab>(entityBuilder, uowFactory, commonService);
			var page = new TdiPage<EntityViewModel>(viewModel, viewModel, "hash_1");

			var tabWidget = Substitute.For<Gtk.Widget>();
			var resolver = Substitute.For<ITDIWidgetResolver>();
			resolver.Resolve(viewModel).Returns(tabWidget);
			var tdiNotebook = Substitute.For<TdiNotebook>();
			tdiNotebook.WidgetResolver = resolver;

			var pageFactory = Substitute.For<IViewModelsPageFactory>();
			pageFactory.CreateViewModelTypedArgs<EntityViewModel>(null, new System.Type[] { }, new object[] { }, "hash_1", null).ReturnsForAnyArgs(page);

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactiveMessage);

			var firstPage = navManager.OpenViewModel<EntityViewModel>(null);

			DialogViewModelBase dialogViewModel = viewModel;
			var find = navManager.FindPage(dialogViewModel);

			Assert.That(find, Is.EqualTo(firstPage));
		}

		[Test(Description = "Тест остановки создания вкладки.")]
		public void OpenViewModel_AbortingVMCreationTest()
		{
			var tdiNotebook = Substitute.For<TdiNotebook>();
			var hashGenerator = new ClassNamesHashGenerator(null);
			var interactive = Substitute.For<IInteractiveMessage>();

			var builder = new ContainerBuilder();
			builder.RegisterType<AbortCreationViewModel>().AsSelf();

			var pageFactory = new AutofacViewModelsTdiPageFactory(builder.Build());

			var navManager = new TdiNavigationManager(tdiNotebook, hashGenerator, pageFactory, interactive);

			var page = navManager.OpenViewModel<AbortCreationViewModel, INavigationManager>(null, navManager);

			interactive.Received().ShowMessage(Arg.Any<ImportanceLevel>(), "Вкладка не создана!", "Остановка");
		}

		#region Тестирование ISlideableViewModel

		[Test(Description = "Тест открытия вкладки журнала внутри слайдера, если мастер вкладка с UseSlider = true.")]
		public void OpenViewModel_ISlideableViewModel_InSliderTest()
		{
			var builder = new ContainerBuilder();
			IContainer container = null;
			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(container)).As<IViewModelsPageFactory>();
			var resolver = Substitute.For<IGtkViewResolver>();
			resolver.Resolve(Arg.Any<SlideableViewModel>()).ReturnsForAnyArgs((arg) => new EmptyDialogView());
			builder.RegisterInstance<IGtkViewResolver>(resolver).As<IGtkViewResolver>();
			builder.Register(x => Substitute.For<ITdiPageFactory>()).As<ITdiPageFactory>();
			builder.Register(x => Substitute.For<IInteractiveService>()).As<IInteractiveService>();
			builder.Register(x => Substitute.For<IInteractiveMessage>()).As<IInteractiveMessage>();
			builder.RegisterType<SlideableViewModel>().AsSelf();
			container = builder.Build();

			var notebook = new TdiNotebook();
			var navigation = container.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), notebook));
			var parameters = new Dictionary<string, object>(); 
			parameters["useSlider"] = true;
			var slidePage = navigation.OpenViewModelNamedArgs<SlideableViewModel>(null, parameters, OpenPageOptions.IgnoreHash);
			parameters["useSlider"] = false;
			var slidePage2 = navigation.OpenViewModelNamedArgs<SlideableViewModel>(slidePage.ViewModel, parameters, OpenPageOptions.IgnoreHash);

			Assert.That(slidePage.ViewModel, Is.Not.EqualTo(slidePage2.ViewModel));
			Assert.That(notebook.Tabs.Count, Is.EqualTo(1));
			Assert.That(notebook.Tabs[0].TdiTab, Is.InstanceOf<TdiSliderTab>());
			var slider = notebook.Tabs[0].TdiTab as TdiSliderTab;
			Assert.That(slider.Journal, Is.Not.Null);
			Assert.That(slider.ActiveDialog, Is.Not.Null);
		}

		[Test(Description = "Тест открытия вкладки журнала внутри слайдера, если мастер вкладка с UseSlider = true. Доп тест в случае если вторая вкладака тоже UseSlider = true.")]
		public void OpenViewModel_ISlideableViewModel_InSlider2Test()
		{
			var builder = new ContainerBuilder();
			IContainer container = null;
			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(container)).As<IViewModelsPageFactory>();
			var resolver = Substitute.For<IGtkViewResolver>();
			resolver.Resolve(Arg.Any<SlideableViewModel>()).ReturnsForAnyArgs((arg) => new EmptyDialogView());
			builder.RegisterInstance<IGtkViewResolver>(resolver).As<IGtkViewResolver>();
			builder.Register(x => Substitute.For<ITdiPageFactory>()).As<ITdiPageFactory>();
			builder.Register(x => Substitute.For<IInteractiveService>()).As<IInteractiveService>();
			builder.Register(x => Substitute.For<IInteractiveMessage>()).As<IInteractiveMessage>();
			builder.RegisterType<SlideableViewModel>().AsSelf();
			container = builder.Build();

			var notebook = new TdiNotebook();
			var navigation = container.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), notebook));

			var parameters = new Dictionary<string, object>();
			parameters["useSlider"] = true;
			var slidePage = navigation.OpenViewModelNamedArgs<SlideableViewModel>(null, parameters, OpenPageOptions.IgnoreHash);
			parameters["useSlider"] = true;
			var slidePage2 = navigation.OpenViewModelNamedArgs<SlideableViewModel>(slidePage.ViewModel, parameters, OpenPageOptions.IgnoreHash);

			Assert.That(slidePage.ViewModel, Is.Not.EqualTo(slidePage2.ViewModel));
			Assert.That(notebook.Tabs.Count, Is.EqualTo(1));
			Assert.That(notebook.Tabs[0].TdiTab, Is.InstanceOf<TdiSliderTab>());
			var slider = notebook.Tabs[0].TdiTab as TdiSliderTab;
			Assert.That(slider.Journal, Is.Not.Null);
			Assert.That(slider.ActiveDialog, Is.Not.Null);
		}

		[Test(Description = "Проверка что вкладка журнала не должна открываться в слайдере при установленном у нее UseSlider = false.")]
		public void OpenViewModel_ISlideableViewModel_DisableInJournalSliderTest()
		{
			var builder = new ContainerBuilder();
			IContainer container = null;
			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(container)).As<IViewModelsPageFactory>();
			var resolver = Substitute.For<IGtkViewResolver>();
			resolver.Resolve(Arg.Any<SlideableViewModel>()).ReturnsForAnyArgs((arg) => new EmptyDialogView());
			builder.RegisterInstance<IGtkViewResolver>(resolver).As<IGtkViewResolver>();
			builder.Register(x => Substitute.For<ITdiPageFactory>()).As<ITdiPageFactory>();
			builder.Register(x => Substitute.For<IInteractiveService>()).As<IInteractiveService>();
			builder.Register(x => Substitute.For<IInteractiveMessage>()).As<IInteractiveMessage>();
			builder.RegisterType<SlideableViewModel>().AsSelf();
			container = builder.Build();

			var notebook = new TdiNotebook();
			var navigation = container.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), notebook));

			var parameters = new Dictionary<string, object>();
			parameters["useSlider"] = false;
			var slidePage = navigation.OpenViewModelNamedArgs<SlideableViewModel>(null, parameters, OpenPageOptions.IgnoreHash);
			parameters["useSlider"] = true;
			var slidePage2 = navigation.OpenViewModelNamedArgs<SlideableViewModel>(slidePage.ViewModel, parameters, OpenPageOptions.IgnoreHash);

			Assert.That(slidePage.ViewModel, Is.Not.EqualTo(slidePage2.ViewModel));
			Assert.That(notebook.Tabs.Count, Is.EqualTo(2));
			Assert.That(notebook.Tabs[0].TdiTab, Is.Not.InstanceOf<TdiSliderTab>());
		}

		[Test(Description = "Проверка что вкладка диалога не открывается внутри слайдера если у нее AlwaysNewPage = true.")]
		public void OpenViewModel_ISlideableViewModel_DisableInDialogSliderTest()
		{
			var builder = new ContainerBuilder();
			IContainer container = null;
			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(container)).As<IViewModelsPageFactory>();
			var resolver = Substitute.For<IGtkViewResolver>();
			resolver.Resolve(Arg.Any<SlideableViewModel>()).ReturnsForAnyArgs((arg) => new EmptyDialogView());
			builder.RegisterInstance<IGtkViewResolver>(resolver).As<IGtkViewResolver>();
			builder.Register(x => Substitute.For<ITdiPageFactory>()).As<ITdiPageFactory>();
			builder.Register(x => Substitute.For<IInteractiveService>()).As<IInteractiveService>();
			builder.Register(x => Substitute.For<IInteractiveMessage>()).As<IInteractiveMessage>();
			builder.RegisterType<SlideableViewModel>().AsSelf();
			container = builder.Build();

			var notebook = new TdiNotebook();
			var navigation = container.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), notebook));

			var parameters = new Dictionary<string, object>();
			parameters["useSlider"] = true;
			var slidePage = navigation.OpenViewModelNamedArgs<SlideableViewModel>(null, parameters, OpenPageOptions.IgnoreHash);
			parameters["useSlider"] = false;
			parameters["alwaysNewPage"] = true;
			var slidePage2 = navigation.OpenViewModelNamedArgs<SlideableViewModel>(slidePage.ViewModel, parameters, OpenPageOptions.IgnoreHash);

			Assert.That(slidePage.ViewModel, Is.Not.EqualTo(slidePage2.ViewModel));
			Assert.That(notebook.Tabs.Count, Is.EqualTo(2));
			Assert.That(notebook.Tabs[0].TdiTab, Is.InstanceOf<TdiSliderTab>());
		}

		[Test(Description = "Проверка что вкладка диалога не открывается внутри слайдера если у нее AlwaysNewPage = true. Второй тест на случай если вторая вкадка тоже UseSlider = true")]
		public void OpenViewModel_ISlideableViewModel_DisableInDialogSliderTest2()
		{
			var builder = new ContainerBuilder();
			IContainer container = null;
			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(container)).As<IViewModelsPageFactory>();
			var resolver = Substitute.For<IGtkViewResolver>();
			resolver.Resolve(Arg.Any<SlideableViewModel>()).ReturnsForAnyArgs((arg) => new EmptyDialogView());
			builder.RegisterInstance<IGtkViewResolver>(resolver).As<IGtkViewResolver>();
			builder.Register(x => Substitute.For<ITdiPageFactory>()).As<ITdiPageFactory>();
			builder.Register(x => Substitute.For<IInteractiveService>()).As<IInteractiveService>();
			builder.Register(x => Substitute.For<IInteractiveMessage>()).As<IInteractiveMessage>();
			builder.RegisterType<SlideableViewModel>().AsSelf();
			container = builder.Build();

			var notebook = new TdiNotebook();
			var navigation = container.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), notebook));

			var parameters = new Dictionary<string, object>();
			parameters["useSlider"] = true;
			var slidePage = navigation.OpenViewModelNamedArgs<SlideableViewModel>(null, parameters, OpenPageOptions.IgnoreHash);
			parameters["useSlider"] = true;
			parameters["alwaysNewPage"] = true;
			var slidePage2 = navigation.OpenViewModelNamedArgs<SlideableViewModel>(slidePage.ViewModel, parameters, OpenPageOptions.IgnoreHash);

			Assert.That(slidePage.ViewModel, Is.Not.EqualTo(slidePage2.ViewModel));
			Assert.That(notebook.Tabs.Count, Is.EqualTo(2));
			Assert.That(notebook.Tabs[0].TdiTab, Is.InstanceOf<TdiSliderTab>());
		}
	}

	#endregion
}
