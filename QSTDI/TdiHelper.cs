using System;
using Gtk;

namespace QSTDI
{
	public static class TdiHelper
	{
		public static ITdiTab FindMyTab(Widget child)
		{
			if (child.Parent is ITdiTab)
				return child.Parent as ITdiTab;
			else if (child.Parent.IsTopLevel)
				return null;
			else
				return FindMyTab(child.Parent);
		}
	}
}

