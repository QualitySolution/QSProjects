using System;
using System.ComponentModel;
using GammaBinding.ColumnConfig;
using Gtk;

namespace GammaBinding.Gtk
{
	[ToolboxItem (true)]
	public class yTreeView : TreeView
	{
		IColumnsConfig columnsConfig;
		public IColumnsConfig ColumnsConfig {
			get {
				return columnsConfig;
			}
			set {
				columnsConfig = value;
			}
		}

		public yTreeView ()
		{
		}
	}
}

