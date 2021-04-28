using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Navigation;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Test.GtkUI;
using QS.Test.TestApp.Dialogs;

namespace QS.Test.Tdi.GtkUI
{
	[TestFixture(TestOf = typeof(TdiNotebook))]
	public class TdiNotebookTest
	{
		[Test(Description = "Проверяем что вкладка в момент закрытия действительно получает вызов метода OnTabClosed от notebook")]
		public void TdiTabClosedEventTest()
		{
			var tab = Substitute.For<ITdiTab, Gtk.Widget>();

			var notebook = new TdiNotebook();

			notebook.AddTab(tab);

			Assert.That(notebook.Tabs.First().TdiTab, Is.EqualTo(tab));

			notebook.ForceCloseTab(tab, CloseSource.External);
			tab.Received().OnTabClosed();
			Assert.That(notebook.Tabs.Count, Is.EqualTo(0));
		}

		[Test(Description = "Проверяем что вкладка в момент закрытия действительно получает вызов метода OnTabClosed от notebook, при условии открытия через слайдер.")]
		public void TdiTabClosedEventWithSliderTest()
		{
			var tab = Substitute.For<ITdiJournal, Gtk.Widget>();
			tab.UseSlider.Returns(true);

			var notebook = new TdiNotebook();

			notebook.AddTab(tab);

			Assert.That(notebook.Tabs.First().TdiTab, Is.InstanceOf<TdiSliderTab>());

			notebook.ForceCloseTab(tab, CloseSource.External);
			tab.Received().OnTabClosed();
			Assert.That(notebook.Tabs.Count, Is.EqualTo(0));
		}

		[Test(Description = "Проверяем что вкладка в момент закрытия действительно получает вызов метода OnTabClosed от notebook, при условии открытия через слайдер.")]
		public void TdiTabClosedEventForDialogInSliderTest()
		{
			GtkInit.AtOnceInitGtk();
			bool notebookEventRised = false, tabEventRised = false;

			var tabJournal = new EmptyJournalTab();
			tabJournal.UseSlider = true;

			var tab = new EmptyDlg();
			tab.TabClosed += (sender, e) => tabEventRised = true;

			var notebook = new TdiNotebook();
			notebook.TabClosed += (sender, e) => notebookEventRised = (e.Tab == tab);

			notebook.AddTab(tabJournal);
			tabJournal.TabParent.AddTab(tab, tabJournal);

			var slider = tabJournal.TabParent as TdiSliderTab;

			Assert.That(slider.ActiveDialog, Is.EqualTo(tab));
			notebook.ForceCloseTab(tab, CloseSource.External);

			Assert.That(slider.ActiveDialog, Is.Null);
			Assert.That(tabEventRised, Is.True);
			Assert.That(notebookEventRised, Is.True);
		}
	}
}
