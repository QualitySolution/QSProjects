using System;

namespace Gamma.Widgets
{
	public class ItemSelectedEventArgs : EventArgs
	{
		public object SelectedItem { get; private set; }

		public ItemSelectedEventArgs(object item)
		{
			SelectedItem = item;
		}
	}
}

