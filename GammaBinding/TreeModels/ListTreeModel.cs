using System;
using System.Collections;
using System.Runtime.InteropServices;
using Gamma.Binding;
using Gtk;

namespace Gamma.Binding
{
	public class ListTreeModel : GLib.Object, TreeModelImplementor, IyTreeModel
	{
		TreeModel adapter;
		IList sourceList;
		IEnumerator cachedEnumerator;

		public event EventHandler RenewAdapter;

		public ListTreeModel (IList list)
		{
			adapter = new TreeModelAdapter (this);
			sourceList = list;
		}

		protected IList SourceList {
			get {
				return sourceList;
			}
		}

		#region IyTreeModel implementation

		public TreeModel Adapter {
			get { return adapter;
			}
		}

		#endregion

		#region TreeModelImplementor implementation

		public GLib.GType GetColumnType (int index_)
		{
			return GLib.GType.Object;
		}

		public bool GetIter (out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			iter = TreeIter.Zero;

			object node = NodeAtPath (path); //FIXME Will be optimized
			if (node == null)
				return (false);

			iter = IterFromNode (node);
			return (true);
		}

		public TreePath GetPath (TreeIter iter)
		{
			object node = NodeFromIter (iter);
			if (node == null) 
				throw new ArgumentException ("iter");

			return (PathFromNode (node));
		}

		public void GetValue (TreeIter iter, int column, ref GLib.Value value)
		{
			value = new GLib.Value(NodeFromIter(iter));
		}

		public bool IterNext (ref TreeIter iter)
		{
			object node = NodeFromIter (iter);
			if ((node == null) || (sourceList == null) || (sourceList.Count == 0))
				return (false);
			object lastNode;
			//Check for "Collection was modified" Exception
			try { 
				lastNode = cachedEnumerator != null ? cachedEnumerator.Current : null;
			} catch (InvalidOperationException ex) {
				lastNode = null;
			}
			if (lastNode == node)
				return GetCacheNext (ref iter);
			else {
				cachedEnumerator = sourceList.GetEnumerator ();
				while (cachedEnumerator.MoveNext ()) {
					if (node == cachedEnumerator.Current)
						return GetCacheNext (ref iter);
				}
				cachedEnumerator = null;
				return false;
			}
		}

		public bool IterChildren (out TreeIter iter, TreeIter parent)
		{
			throw new NotImplementedException ();
		}

		public bool IterHasChild (TreeIter iter)
		{
			throw new NotImplementedException ();
		}

		public int IterNChildren (TreeIter iter)
		{
			if (iter.Equals (TreeIter.Zero))
				return SourceList.Count;
			else
				return 0;
		}

		public bool IterNthChild (out TreeIter iter, TreeIter parent, int n)
		{
			iter = TreeIter.Zero;
			if (sourceList == null || sourceList.Count == 0)
				return false;

			if (parent.UserData == IntPtr.Zero) {
				if (sourceList.Count <= n)
					return (false);
				iter = IterFromNode (sourceList [n]);
				return (true);
			}
			return false;
		}

		public bool IterParent (out TreeIter iter, TreeIter child)
		{
			iter = TreeIter.Zero;
			return false;
		}

		public void RefNode (TreeIter iter)
		{
			
		}

		public void UnrefNode (TreeIter iter)
		{
			
		}

		public TreeModelFlags Flags {
			get {
				return TreeModelFlags.ListOnly;
			}
		}

		public int NColumns {
			get { return 1;
			}
		}

		#endregion

		public object NodeAtPath (TreePath aPath)
		{
			if (sourceList == null)
				return (null);
			if (aPath.Indices.Length == 0)
				return (null);
			if (aPath.Indices [0] < 0 || aPath.Indices [0] >= sourceList.Count)
				return null;
			return (sourceList [aPath.Indices [0]]);
		}

		Hashtable node_hash = new Hashtable ();

		public TreeIter IterFromNode (object node)
		{
			GCHandle gch;
			if (node_hash [node] != null) {
				gch = (GCHandle) node_hash [node];
			} else {
				gch = GCHandle.Alloc (node);
				node_hash [node] = gch;
			}
			TreeIter result = TreeIter.Zero;
			result.UserData = (IntPtr) gch;
			return result;
		}

		public object NodeFromIter (TreeIter iter)
		{
			GCHandle gch = (GCHandle) iter.UserData;
			return gch.Target;
		}

		public TreePath PathFromNode (object aNode)
		{
			TreePath tp = new TreePath ();
			if ((aNode == null) || (sourceList == null) || (sourceList.Count == 0))
				return (tp);

			int i = sourceList.IndexOf (aNode);
			if (i > -1)
				tp.AppendIndex (i);
			return (tp);
		}

		private bool GetCacheNext (ref TreeIter iter)
		{
			if (cachedEnumerator.MoveNext ()) {
				iter = IterFromNode (cachedEnumerator.Current);
				return true;
			} else {
				cachedEnumerator = null;
				return false;
			}
		}


	}
}

