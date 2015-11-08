using System;
using Gtk;

namespace Gamma.Binding
{
	public interface IyTreeModel : TreeModelImplementor
	{
		TreeModel Adapter { get;}

		object NodeAtPath(TreePath aPath);
		object NodeFromIter (TreeIter iter);

		event EventHandler RenewAdapter;
	}
}

