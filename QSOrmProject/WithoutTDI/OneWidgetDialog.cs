using System;
using System.ComponentModel;
using Gtk;
using QSTDI;

namespace QSOrmProject
{
	public partial class OneWidgetDialog : FakeTDITabGtkDialogBase
	{
		ITdiTab tdiTab;

		public OneWidgetDialog (Widget widget)
		{
			this.Build ();

			var att = widget.GetType ().GetCustomAttributes (typeof(WidgetWindowAttribute), false);
			if (att.Length > 0)
				this.SetDefaultSize ((att [0] as WidgetWindowAttribute).DefaultWidth,
				                     (att [0] as WidgetWindowAttribute).DefaultHeight);

			widget.Show ();
			VBox.Add (widget);

			att = widget.GetType ().GetCustomAttributes (typeof(DisplayNameAttribute), true);
			if (att.Length > 0)
				Title = (att [0] as DisplayNameAttribute).DisplayName;

			tdiTab = widget as ITdiTab;
			if(tdiTab != null)
			{
				if(!String.IsNullOrWhiteSpace (tdiTab.TabName))
					Title = tdiTab.TabName;
				tdiTab.TabParent = this;
				tdiTab.TabNameChanged += TdiTab_TabNameChanged;
				tdiTab.CloseTab += TdiTab_CloseTab;
			}

			this.ReshowWithInitialSize ();
		}

		void TdiTab_CloseTab (object sender, TdiTabCloseEventArgs e)
		{
			Respond (ResponseType.Close);
		}

		void TdiTab_TabNameChanged (object sender, TdiTabNameChangedEventArgs e)
		{
			Title = tdiTab.TabName;
		}

		protected override void OnDestroyed ()
		{
			(tdiTab as Widget).Destroy ();
			base.OnDestroyed ();
		}
	}
}

