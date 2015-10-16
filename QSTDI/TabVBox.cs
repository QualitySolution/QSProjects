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
			titleLabel = new Label ();
			if(tab is ITdiTabWithPath)
			{
				(tab as ITdiTabWithPath).PathChanged += OnPathUpdated;
				OnPathUpdated (null, EventArgs.Empty);
			}
			else
			{
				tab.TabNameChanged += Tab_TabNameChanged;
				Tab_TabNameChanged (null, null);
			}

			this.PackStart (titleLabel, false, true, 2);
			this.Add ((Widget)tab);
			titleLabel.Show ();
			(tab as Widget).Show ();
		}

		void OnPathUpdated (object sender, EventArgs e)
		{
			titleLabel.Markup = String.Format ("<b>{0}</b>", String.Join (" <span color='blue'>➤</span> ", (tab as ITdiTabWithPath).PathNames));
		}

		void Tab_TabNameChanged (object sender, TdiTabNameChangedEventArgs e)
		{
			titleLabel.Markup = String.Format ("<b>{0}</b>", tab.TabName);
		}
	}
}

