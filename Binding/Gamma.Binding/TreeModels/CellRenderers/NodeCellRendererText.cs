using System;
using System.Collections.Generic;
using Gtk;
using System.Reflection;
using Gamma.Binding;
using System.Text.RegularExpressions;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererText<TNode> : CellRendererText, INodeCellRendererHighlighter, ISelfGetNodeRenderer
	{
		public List<Action<NodeCellRendererText<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererText<TNode>, TNode>>();

		public bool SearchHighlight { get; set;}

		public PropertyInfo DataPropertyInfo { get; set;}

		public IValueConverter EditingValueConverter { get; set;}
		
		public Func<string, object> GetNodeFunc { set; private get; }
		public void RenderNode(object node)
		{
			if(node is TNode)
			{
				var typpedNode = (TNode)node;
				LambdaSetters.ForEach (a => a.Invoke (this, typpedNode));
			}
		}

		public void RenderNode(object node, string[] searchHighlightTexts)
		{
			RenderNode(node);
			if (SearchHighlight && !String.IsNullOrEmpty(Text) && searchHighlightTexts != null && searchHighlightTexts.Length > 0)
			{
				string resultMarkup = Text;
				foreach(var searchText in searchHighlightTexts)
				{
					string pattern = Regex.Escape(searchText.ToLower());
					resultMarkup = Regex.Replace(resultMarkup, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
				}
				Markup = resultMarkup;
			}
		}

		protected override void OnEdited(string path, string new_text) {
			var node = GetNodeFunc(path);
			if(DataPropertyInfo != null && DataPropertyInfo.CanWrite) {
				DataPropertyInfo.SetValue(node, new_text, null);
			}
		}
	}
}

