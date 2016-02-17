using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Gamma.Binding;
using Gamma.Utilities;
using Gtk;

namespace Gamma.Binding
{
	public class RecursiveTreeModel<TNode> : GLib.Object, TreeModelImplementor, IyTreeModel
	{
		TreeModel adapter;
		IList<TNode> sourceList;
		IEnumerator cachedEnumerator;

		PropertyInfo parentProperty;
		PropertyInfo childsCollectionProperty;

		public event EventHandler RenewAdapter;

		public RecursiveTreeModel (IList<TNode> list, Expression<Func<TNode, TNode>> parentPropertyExpr, Expression<Func<TNode, IList<TNode>>> childsCollectionPropertyExpr)
		{
			parentProperty = PropertyUtil.GetPropertyInfo(parentPropertyExpr);
			childsCollectionProperty = PropertyUtil.GetPropertyInfo(childsCollectionPropertyExpr);

			adapter = new TreeModelAdapter (this);
			sourceList = list;
		}

		public RecursiveTreeModel (IList<TNode> list, IRecursiveTreeConfig treeConfig)
		{
			parentProperty = treeConfig.ParentProperty;
			childsCollectionProperty = treeConfig.ChildsCollectionProperty;

			adapter = new TreeModelAdapter (this);
			sourceList = list;
		}

		protected IList<TNode> SourceList {
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
				var parent = parentProperty.GetValue(node, null);
				if (parent == null)
					cachedEnumerator = sourceList.GetEnumerator();
				else
					cachedEnumerator = (childsCollectionProperty.GetValue(parent, null) as IList).GetEnumerator();

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
			iter = TreeIter.Zero;
			var list = GetChildsList(parent);
			if (list == null && list.Count == 0)
				return false;

			iter = IterFromNode(list[0]);
			return true;
		}

		public bool IterHasChild (TreeIter iter)
		{
			var list = GetChildsList(iter);
			return list != null && list.Count > 0;
		}

		public int IterNChildren (TreeIter iter)
		{
			if (iter.Equals (TreeIter.Zero))
				return SourceList.Count;
			else
			{
				var list = GetChildsList(iter);
				return list != null ? list.Count : 0;
			}
		}

		public bool IterNthChild (out TreeIter iter, TreeIter parent, int n)
		{
			iter = TreeIter.Zero;
			if (sourceList == null || sourceList.Count == 0)
				return false;

			var list = parent.UserData == IntPtr.Zero ? (IList)sourceList : GetChildsList(parent);

			if (list == null || list.Count <= n)
				return false;

			iter = IterFromNode (list [n]);
			return true;
		}

		public bool IterParent (out TreeIter iter, TreeIter child)
		{
			iter = TreeIter.Zero;
			var node = NodeFromIter(child);
			var parent = parentProperty.GetValue(node, null);
			if(parent == null)
				return false;

			iter = IterFromNode(parent);
			return true;
		}

		public void RefNode (TreeIter iter)
		{
			
		}

		public void UnrefNode (TreeIter iter)
		{
			
		}

		public TreeModelFlags Flags {
			get {
				return TreeModelFlags.ItersPersist;
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
			var item = sourceList[aPath.Indices[0]];

			if(aPath.Depth == 1)
				return item;
			
			return GetLevelNode(item, aPath, 1);
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

			var curNode = (TNode)aNode;
			var indicesList = new List<int>();

			do
			{
				var parent = (TNode)parentProperty.GetValue(curNode, null);
				if(parent == null)
				{
					int i = sourceList.IndexOf (curNode);
					if (i == -1)
						return tp;
					indicesList.Add(i);
					indicesList.Reverse();
					indicesList.ForEach(tp.AppendIndex);
					return tp;
				}

				var curList = (IList<TNode>)childsCollectionProperty.GetValue(parent, null);
				int ix = curList.IndexOf(curNode);
				if (ix == -1)
					return tp;

				indicesList.Add(ix);
				curNode = parent;
			} while (true);
		}
		
	#region Privates

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

		private IList GetChildsList(TreeIter iter)
		{
			var node = NodeFromIter(iter);
			return childsCollectionProperty.GetValue(node, null) as IList;
		}

		private object GetLevelNode(object parentNode, TreePath aPath, int level)
		{
			var childs = childsCollectionProperty.GetValue(parentNode, null) as IList;

			if (aPath.Indices [level] < 0 || aPath.Indices [level] >= childs.Count)
				return null;

			if (aPath.Depth > level + 1)
				return GetLevelNode(childs[aPath.Indices[level]], aPath, level + 1);
			else
				return childs[aPath.Indices[level]];
		}

	#endregion

	}
}

