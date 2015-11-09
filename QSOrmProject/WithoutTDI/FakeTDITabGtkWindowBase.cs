using System;
using QSTDI;

namespace QSOrmProject
{
	public class FakeTDITabGtkWindowBase : Gtk.Window, ITdiTab, ITdiTabParent
	{
		public FakeTDITabGtkWindowBase (Gtk.WindowType type):base(type)
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

		public bool FailInitialize { get; protected set;}

		public ITdiTabParent TabParent {
			get { return this; }
			set { throw new NotSupportedException (); }
		}

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

		#region ITdiTabParent implementation
		public void AddSlaveTab (ITdiTab masterTab, ITdiTab slaveTab, bool CanSlided = true)
		{
			RunDlg (slaveTab);
		}

		public void AddTab (ITdiTab tab, ITdiTab afterTab, bool CanSlided = true)
		{
			RunDlg (tab);
		}

		void RunDlg(ITdiTab dlg)
		{
			if (dlg is Gtk.Dialog) {
				var window = dlg as Gtk.Dialog;
				window.Show ();
				window.Run ();
				window.Destroy ();
			} else if (dlg is Gtk.Widget) {
				var window = new OneWidgetDialog (dlg as Gtk.Widget);
				window.Show ();
				window.Run ();
				window.Destroy ();
			} else
				throw new NotImplementedException ();
		}

		public bool CheckClosingSlaveTabs (ITdiTab tab)
		{
			return true;
		}

		public TdiBeforeCreateResultFlag BeforeCreateNewTab (object subject, ITdiTab masterTab, bool CanSlided = true)
		{
			return TdiBeforeCreateResultFlag.Ok;
		}

		public TdiBeforeCreateResultFlag BeforeCreateNewTab (Type subjectType, ITdiTab masterTab, bool CanSlided = true)
		{
			return TdiBeforeCreateResultFlag.Ok;
		}
		#endregion
	}
}

