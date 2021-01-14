using System;
using Gtk;
using QS.Tdi;

namespace QS.Dialog.Gtk
{
	public abstract class WidgetOnTdiTabBase : Bin
	{
		ITdiTab mytab;

		protected ITdiTab MyTab {
			get {
				if(mytab == null)
					mytab = DialogHelper.FindParentTab(this);
				if(mytab == null) {
					throw new InvalidOperationException("Родительская вкладка не найдена.");
				}
				else
					return mytab;
			}
		}

		protected override void OnParentSet(Widget previous_parent)
		{
			mytab = null;
			base.OnParentSet(previous_parent);
		}

		protected void OpenSlaveTab(ITdiTab slaveTab)
		{
			MyTab.TabParent.AddSlaveTab(MyTab, slaveTab);
		}

		protected void OpenNewTab(ITdiTab tab)
		{
			MyTab.TabParent.AddTab(tab, MyTab);
		}

		protected ITdiTab OpenTab<TTab>() where TTab : ITdiTab
		{
			return MyTab.TabParent.OpenTab<TTab>(MyTab);
		}

		protected ITdiTab OpenTab<TTab, TArg1>(TArg1 arg1) where TTab : ITdiTab
		{
			return MyTab.TabParent.OpenTab<TTab, TArg1>(arg1, MyTab);
		}

		protected ITdiTab OpenTab<TTab, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where TTab : ITdiTab
		{
			return MyTab.TabParent.OpenTab<TTab, TArg1, TArg2>(arg1, arg2, MyTab);
		}

		protected ITdiTab OpenTab<TTab, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) where TTab : ITdiTab
		{
			return MyTab.TabParent.OpenTab<TTab, TArg1, TArg2, TArg3>(arg1, arg2, arg3, MyTab);
		}
	}
}

