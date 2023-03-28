using System;
using System.Collections.Generic;
using Gtk;
using System.Reflection;
using Gamma.Binding;
using System.Text.RegularExpressions;
using Gdk;

namespace Gamma.GtkWidgets.Cells
{
	public class NodeCellRendererDate<TNode> : CellRendererText, INodeCellRendererHighlighter, ISelfGetNodeRenderer
	{
		public List<Action<NodeCellRendererDate<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererDate<TNode>, TNode>>();

		public bool SearchHighlight { get; set;}

		public PropertyInfo DataPropertyInfo { get; set;}

		public IValueConverter EditingValueConverter { get; set;}
		
		public Func<string, object> GetNodeFunc { private get; set; }

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
				if(String.IsNullOrWhiteSpace(new_text)) {
					if(DataPropertyInfo.PropertyType.IsAssignableFrom(typeof(DateTime?)))
						DataPropertyInfo.SetValue(node, null, null);
				}
				else {
					if(DateTime.TryParse(new_text, out DateTime date))
						DataPropertyInfo.SetValue(node, date, null);
				}

			}
		}

		protected override void OnEditingStarted(CellEditable editable, string path) {
			if(editable is Entry entry) {
				entry.Changed += delegate(object sender, EventArgs args) {
					if(DateTime.TryParse(entry.Text, out DateTime outDate))
						entry.ModifyText(StateType.Normal);
					else
						entry.ModifyText(StateType.Normal, new Color(255,0,0)); 
				};
			}
			base.OnEditingStarted(editable, path);
		}
	}
}

