using System;
using System.ComponentModel;
using Gtk;
using QS.Tdi;

namespace QS.Dialog.Gtk
{
	public class TdiTabBase : Bin, ITdiTab
	{
		public HandleSwitchIn HandleSwitchIn { get; protected set; }
		public HandleSwitchOut HandleSwitchOut { get; protected set; }

		public TdiTabBase ()
		{
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		private string tabName = String.Empty;

		/// <summary>
		/// Имя вкладки может быть автоматически получено из атрибута DisplayNameAttribute у класса диалога.
		/// </summary>
		public virtual string TabName {
			get { 
				if(String.IsNullOrWhiteSpace(tabName))
				{
					var atts = (DisplayNameAttribute[])this.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true);
					if(atts.Length > 0)
					{
						return atts[0].DisplayName;
					}
				}
				return tabName;
			}
			protected set {
				if (tabName == value)
					return;
				tabName = value;
				OnTabNameChanged ();
			}
		}

		public ITdiTabParent TabParent { set; get; }

		public bool FailInitialize { get; protected set;}

		public virtual bool CompareHashName(string hashName)
		{
			return GenerateHashName(this.GetType()) == hashName;
		}

		public static string GenerateHashName<TTab>() where TTab : QS.Dialog.Gtk.TdiTabBase
		{
			return GenerateHashName(typeof(TTab));
		}

		public static string GenerateHashName(Type tabType)
		{
			if (!typeof(TdiTabBase).IsAssignableFrom(tabType))
				throw new ArgumentException("Тип должен наследоваться от TdiTabBase", "tabType");

			return String.Format("Dlg_{0}", tabType.Name);
		}

		#endregion

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		protected void OnTabNameChanged()
		{
			var uowDlg = this as ISingleUoWDialog;
			if(uowDlg?.UoW?.ActionTitle != null)
				uowDlg.UoW.ActionTitle.UserActionTitle = $"Вкладка '{TabName}'";

			if (TabNameChanged != null)
				TabNameChanged (this, new TdiTabNameChangedEventArgs (TabName));
		}

		protected void OpenNewTab(ITdiTab tab)
		{
			TabParent.AddTab (tab, this);
		}

		protected void OpenSlaveTab(ITdiTab slaveTab)
		{
			TabParent.AddSlaveTab(this, slaveTab);
		}
	}
}

