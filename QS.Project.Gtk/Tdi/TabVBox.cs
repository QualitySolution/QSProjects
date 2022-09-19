using System;
using System.Linq;
using Gtk;
using QS.Tdi.Gtk;
using QS.Utilities.Text;

namespace QS.Tdi
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

		public TabVBox (ITdiTab tabWidget, ITDIWidgetResolver widgetResolver)
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

			TabWidget = widgetResolver.Resolve(tabWidget);
			if(TabWidget == null)
				throw new InvalidCastException($"Ошибка приведения типа {nameof(ITdiTab)} к типу {nameof(Widget)}.");

			this.Add (TabWidget);
			titleLabel.Show ();
			TabWidget.Show ();
		}

		void OnPathUpdated (object sender, EventArgs e)
		{
			titleLabel.Markup = String.Format ("<b>{0}</b>", String.Join (" <span color='blue'>➤</span> ", 
				(tab as ITdiTabWithPath).PathNames.Select(x => StringManipulationHelper.EllipsizeMiddle(x, TdiNotebook.MaxTabNameLenght))));
		}

		void Tab_TabNameChanged (object sender, TdiTabNameChangedEventArgs e)
		{
			titleLabel.Markup = String.Format ("<b>{0}</b>", tab.TabName);
		}

		public override void Destroy()
		{
			TabWidget.Destroy();
			base.Destroy();
		}
	}
}

