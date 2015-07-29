using System;

namespace QSTDI
{
	public abstract class WidgetOnTdiTabBase : Gtk.Bin
	{
		ITdiTab mytab;

		protected ITdiTab MyTab{
			get{
				if(mytab == null)
					mytab = TdiHelper.FindMyTab (this);
				if (mytab == null) {
					throw new InvalidOperationException ("Родительская вкладка не найдена.");
				} else
					return mytab;
			}
		}

		protected override void OnParentSet (Gtk.Widget previous_parent)
		{
			mytab = null;
			base.OnParentSet (previous_parent);
		}
	}
}

