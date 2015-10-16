using System;
using Gtk;

namespace QSTDI
{
	public class TabVBox : Gtk.VBox
	{
		ITdiTab tab;
		Label titleLabel;

		public ITdiTab Tab {
			get {
				return tab;
			}
		}

		public TabVBox (ITdiTab tabWidget)
		{
			tab = tabWidget;
			tab.TabNameChanged += Tab_TabNameChanged;
			titleLabel = new Label ();
			titleLabel.Markup = String.Format ("<b>{0}</b>", tab.TabName);
			this.PackStart (titleLabel, false, true, 2);
			this.Add ((Widget)tab);
			titleLabel.Show ();
			(tab as Widget).Show ();
		}

		void Tab_TabNameChanged (object sender, TdiTabNameChangedEventArgs e)
		{
			titleLabel.Markup = String.Format ("<b>{0}</b>", tab.TabName);
		}
	}
}

