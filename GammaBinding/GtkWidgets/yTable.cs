using System;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yTable : Table
	{
		public BindingControler<yTable> Binding { get; private set; }

		public yTable(uint rows, uint columns, bool homogeneous) : base(rows, columns, homogeneous)
		{
			Binding = new BindingControler<yTable>(this);
		}

		public yTable() : base(0,0, false)
		{
			Binding = new BindingControler<yTable>(this);
		}
	}
}
