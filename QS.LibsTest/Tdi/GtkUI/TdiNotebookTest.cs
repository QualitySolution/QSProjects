using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Tdi;
using QS.Tdi.Gtk;

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

			notebook.ForceCloseTab(tab);
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

			notebook.ForceCloseTab(tab);
			tab.Received().OnTabClosed();
			Assert.That(notebook.Tabs.Count, Is.EqualTo(0));
		}
	}
}
