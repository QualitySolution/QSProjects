using System;
using Gtk;
using QS.Tdi;

namespace QS.Dialog.Gtk
{
	public abstract class WidgetOnTdiTabBase : Bin
	{
		ITdiTab mytab;

		protected ITdiTab MyTab{
			get{
				if(mytab == null)
					mytab = DialogHelper.FindParentTab (this);
				if (mytab == null) {
					throw new InvalidOperationException ("Родительская вкладка не найдена.");
				} else
					return mytab;
			}
		}

		protected override void OnParentSet (Widget previous_parent)
		{
			mytab = null;
			base.OnParentSet (previous_parent);
		}

		protected void OpenSlaveTab(ITdiTab slaveTab)
		{
			MyTab.TabParent.AddSlaveTab(MyTab, slaveTab);
		}

		protected void OpenNewTab(ITdiTab tab)
		{
			MyTab.TabParent.AddTab(tab, MyTab);
		}
	}
}

