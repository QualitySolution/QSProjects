using System;
using QSTDI;

namespace QSOrmProject
{
	public abstract class FakeTDITabGtkDialogBase : Gtk.Dialog, ITdiTab, ITdiTabParent
	{
		public FakeTDITabGtkDialogBase ()
		{
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		private string tabName = String.Empty;

		public virtual string TabName {
			get { return tabName;
			}
			set {
				if (tabName == value)
					return;
				tabName = value;
				Title = TabName;
				OnTabNameChanged ();
			}
		}

		public bool FailInitialize { get; protected set;}
			
		public ITdiTabParent TabParent {
			get { return this; }
			set { throw new NotSupportedException (); }
		}
			
		public bool CompareHashName(string hashName)
		{
			return false; //Не имеет значения для файковой вкладки, потому что хеш используется для поиска уже открытой вкладки, для диалога это нельзя использовать.
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

		protected void OnEntitySaved (object entity, bool tabClosed = false)
		{
			if (EntitySaved != null)
				EntitySaved (this, new EntitySavedEventArgs (entity, tabClosed));
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

		public ITdiTab FindTab(string hashName, string masterHashName = null)
		{
			throw new NotImplementedException();
		}

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null)
		{
			var tab = newTabFunc();
			RunDlg (tab);
			return tab;
		}

		public void SwitchOnTab(ITdiTab tab)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

