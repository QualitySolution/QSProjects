using System;
using QS.Navigation;
using QS.Tdi;

namespace QSOrmProject
{
	public class FakeTDITabGtkWindowBase : Gtk.Window, ITdiTab, ITdiTabParent
	{
		public FakeTDITabGtkWindowBase (Gtk.WindowType type):base(type)
		{
		}
		#region ITdiTab implementation
		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }
		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler TabClosed;

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

		public bool CompareHashName(string hashName)
		{
			return false; //Не имеет значения для файковой вкладки, потому что хеш используется для поиска уже открытой вкладки, для диалога это нельзя использовать.
		}

		#endregion

		protected void OnCloseTab (bool askSave, CloseSource source)
		{
			if (askSave)
				TabParent.AskToCloseTab(this, source);
			else
				TabParent.ForceCloseTab(this, source);
		}

		protected void OnTabNameChanged()
		{
			if (TabNameChanged != null)
				TabNameChanged (this, new TdiTabNameChangedEventArgs (TabName));
		}

		#region ITdiTabParent implementation
		public void AddSlaveTab (ITdiTab masterTab, ITdiTab slaveTab)
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

		public ITdiTab OpenTab(Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, Type[] argTypes = null, object[] args = null)
		{
			ITdiTab tab = newTabFunc.Invoke();
			Type tabType = tab.GetType();
			string hashName = TabHashHelper.GetTabHash(tabType, argTypes ?? new Type[] { }, args ?? new object[] { });
			return OpenTab(hashName, () => tab, afterTab);
		}

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, bool canSlided = true)
		{
			var tab = newTabFunc();
			RunDlg (tab);
			return tab;
		}

		public ITdiTab OpenTab<TTab>(ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { }, new object[] { }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1>(TArg1 arg1, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1) }, new object[] { arg1 }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2>(TArg1 arg1, TArg2 arg2, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1, arg2, arg3 }, afterTab);
		}

		public ITdiTab FindTab(string hashName, string masterHashName = null)
		{
			throw new NotImplementedException();
		}

		public void SwitchOnTab(ITdiTab tab)
		{
			throw new NotImplementedException();
		}

		public void OnTabClosed()
		{
			TabClosed?.Invoke(this, EventArgs.Empty);
		}

		public bool AskToCloseTab(ITdiTab tab, CloseSource source)
		{
			throw new NotImplementedException();
		}

		public void ForceCloseTab(ITdiTab tab, CloseSource source)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

