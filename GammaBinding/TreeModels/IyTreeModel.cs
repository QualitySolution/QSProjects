using System;
using Gtk;

namespace Gamma.Binding
{
	public interface IyTreeModel : TreeModelImplementor
	{
		TreeModel Adapter { get;}

		object GetNodeAtPath(TreePath aPath);
		object NodeFromIter (TreeIter iter);
	}
}

