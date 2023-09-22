using Gtk;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Gamma.Binding {
	public class ObservableListTreeModel : GLib.Object, TreeModelImplementor, IyTreeModel
	{
		TreeModel adapter;
		IObservableList sourceList;
		IEnumerator cachedEnumerator;

		public event EventHandler RenewAdapter;

		public ObservableListTreeModel (IObservableList list)
		{
			adapter = new TreeModelAdapter (this);
			sourceList = list;
			sourceList.CollectionChanged += SourceList_CollectionChanged;
			sourceList.PropertyOfElementChanged += SourceList_PropertyOfElementChanged;
		}

		private void SourceList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			switch(e.Action) {
				case NotifyCollectionChangedAction.Add:
					AddElements(e.NewItems, e.NewStartingIndex);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveElements(e.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					ReplaceElements(e.NewItems, e.OldItems, e.NewStartingIndex);
					break;
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
				case NotifyCollectionChangedAction.Move:
					throw new NotSupportedException("Перемещение элементов внутри коллекции не поддерживается");
			}
		}
		
		private void SourceList_PropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			Adapter.EmitRowChanged(PathFromIndex(sourceList.IndexOf(sender)), IterFromNode(sender));
		}

		void AddElements (IList items, int index)
		{
			//Попытка реализовать работу со множеством элементов,
			//хотя по факту всегда приходит событие с одним элементом
			//но теоретически может быть множество

			if(items.Count < 1) {
				throw new InvalidOperationException("В событии добавления элементов в коллекцию отсутствуют добавляемые элементы");
			}

			foreach(var item in items) {
				TreeIter iter = IterFromNode(item);
				TreePath path = PathFromIndex(index);

				Adapter.EmitRowInserted(path, iter);
			}
		}

		void RemoveElements(IList items) {
			//Попытка реализовать работу со множеством элементов,
			//хотя по факту всегда приходит событие с одним элементом
			//но теоретически может быть множество
			
			if(items.Count < 1) {
				throw new InvalidOperationException("В событии удаления элементов из коллекции отсутствуют добавляемые элементы");
			}

			int[] indexes = new int[items.Count];
			for(int i = 0; i < items.Count - 1; i++) {
				indexes[i] = sourceList.IndexOf(items[i]);
			}
			TreePath path = new TreePath(indexes);
			Adapter.EmitRowDeleted(path);
		}

		void ReplaceElements(IList newItems, IList oldItems, int index)
		{
			RemoveElements(oldItems);
			AddElements(newItems, index);
		}

		void Reset() {
			adapter = new TreeModelAdapter(this);
			OnRenewAdapter();
		}

		public void EmitModelChanged() {
			OnRenewAdapter();
		}

		void OnRenewAdapter() {
			if(RenewAdapter != null)
				RenewAdapter(this, EventArgs.Empty);
		}

		protected IObservableList SourceList => sourceList;

		#region IyTreeModel implementation

		public TreeModel Adapter => adapter;

		#endregion

		#region TreeModelImplementor implementation

		public GLib.GType GetColumnType (int index_)
		{
			return GLib.GType.Object;
		}

		public bool GetIter (out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

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
				throw new ArgumentException (nameof(iter));

			return (PathFromNode (node));
		}

		public void GetValue (TreeIter iter, int column, ref GLib.Value value)
		{
			value = new GLib.Value(NodeFromIter(iter));
		}

		public bool IterNext(ref TreeIter iter)
		{
			if((sourceList == null) || (sourceList.Count == 0) || iter.UserData == IntPtr.Zero)
				return (false);

			object node = NodeFromIter(iter);
			if(node == null)
				return (false);
			object lastNode;
			//Check for "Collection was modified" Exception
			try {
				lastNode = cachedEnumerator != null ? cachedEnumerator.Current : null;
			} catch(InvalidOperationException ex) {
				lastNode = null;
			}
			if(lastNode == node){
				try {
					return GetCacheNext(ref iter);
				}
				catch(InvalidOperationException ex)
				{
					Console.WriteLine("Collection was changed");
				}
			}

			cachedEnumerator = sourceList.GetEnumerator ();
			while (cachedEnumerator.MoveNext ()) {
				if (node == cachedEnumerator.Current)
					return GetCacheNext (ref iter);
			}
			cachedEnumerator = null;
			return false;
		}

		public bool IterChildren (out TreeIter iter, TreeIter parent)
		{
			iter = TreeIter.Zero;
			return false;
		}

		public bool IterHasChild (TreeIter iter)
		{
			return false;
		}

		public int IterNChildren (TreeIter iter)
		{
			if (iter.Equals(TreeIter.Zero as object))
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
			var result = TreeIter.Zero;
			result.UserData = GCHandle.ToIntPtr(gch);
			return result;
		}

		public object NodeFromIter (TreeIter iter)
		{
			var gch = GCHandle.FromIntPtr(iter.UserData);
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

		public TreePath PathFromIndex(int index) {
			TreePath tp = new TreePath();
			if((sourceList == null) || (sourceList.Count == 0))
				return (tp);

			if (index > sourceList.Count - 1) {
				throw new IndexOutOfRangeException();
			}

			tp.AppendIndex(index);
			return tp;
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

		public override void Dispose() {
			sourceList.CollectionChanged -= SourceList_CollectionChanged;

			foreach(GCHandle item in node_hash.Values) {
				item.Free();
			}
		}
	}
}

