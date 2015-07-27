using System;
using Gtk;

namespace QSTDI
{
	public class TdiTabBase : Bin, ITdiTab
	{
		public TdiTabBase ()
		{
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		private string tabName = String.Empty;

		public string TabName {
			get { return tabName;
			}
			set {
				if (tabName == value)
					return;
				tabName = value;
				OnTabNameChanged ();
			}
		}

		public ITdiTabParent TabParent { set; get; }

		#endregion

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		protected void OnTabNameChanged()
		{
			if (TabNameChanged != null)
				TabNameChanged (this, new TdiTabNameChangedEventArgs (TabName));
		}
	}
}

