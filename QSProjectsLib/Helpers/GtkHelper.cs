using System.Collections.Generic;
using Gtk;
using System.Linq;

namespace QSProjectsLib.Helpers
{
	public static class GtkHelper
	{
		public static IEnumerable<Widget> EnumerateAllChildren(Container parent)
		{
			foreach(var child in parent.Children)
			{
				yield return child;
				var container = child as Container;
				if(container != null)
				{
					foreach (var childOfChild in EnumerateAllChildren (container))
					{
						yield return childOfChild;
					}
				}
			}
		}
	}
}
