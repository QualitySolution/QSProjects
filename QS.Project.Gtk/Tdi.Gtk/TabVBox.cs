using System;
using Gtk;

namespace QS.Tdi.Gtk
{
	public class TabVBox : VBox
	{
		public Widget TabWidget { get; }
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

			TabWidget = TDIMain.TDIWidgetResolver.Resolve(tabWidget);
			this.Add (TabWidget);
			titleLabel.Show ();
			TabWidget.Show ();
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

