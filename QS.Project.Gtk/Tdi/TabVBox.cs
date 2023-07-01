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
		Label titleLabel;

		public ITdiTab Tab { get; }

		public TabVBox(ITdiTab tabWidget, ITDIWidgetResolver widgetResolver)
		{
			Tab = tabWidget;
			titleLabel = new Label ();
			if(Tab is ITdiTabWithPath tdiTabWithPath)
			{
				tdiTabWithPath.PathChanged += OnPathUpdated;
				OnPathUpdated(null, EventArgs.Empty);
			}
			else
			{
				Tab.TabNameChanged += Tab_TabNameChanged;
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
			titleLabel.Markup = String.Format("<b>{0}</b>",
				String.Join(" <span color='blue'>➤</span> ",
					(Tab as ITdiTabWithPath).PathNames.Select(x => StringManipulationHelper.EllipsizeMiddle(x, TdiNotebook.MaxTabNameLenght))));
		}

		void Tab_TabNameChanged (object sender, TdiTabNameChangedEventArgs e)
		{
			titleLabel.Markup = $"<b>{Tab.TabName}</b>";
		}

		public override void Destroy()
		{
			TabWidget.Destroy();
			if(Tab is ITdiTabWithPath tdiTabWithPath)
			{
				tdiTabWithPath.PathChanged -= OnPathUpdated;
			}
			else
			{
				Tab.TabNameChanged -= Tab_TabNameChanged;
			}
			base.Destroy();
		}
	}
}

