using System;

namespace Gamma.Widgets
{
	public class EnumItemClickedEventArgs : EventArgs
	{
		public object ItemEnum { get; private set; }

		public EnumItemClickedEventArgs(object item)
		{
			ItemEnum = item;
		}
	}
}

