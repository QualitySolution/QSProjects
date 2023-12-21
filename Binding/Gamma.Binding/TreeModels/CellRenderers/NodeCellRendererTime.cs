using Gamma.Binding;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gamma.GtkWidgets.Cells {
	public class NodeCellRendererTime<TNode> : CellRendererText, INodeCellRendererHighlighter, ISelfGetNodeRenderer {
		public List<Action<NodeCellRendererTime<TNode>, TNode>> LambdaSetters = new List<Action<NodeCellRendererTime<TNode>, TNode>>();

		public bool SearchHighlight { get; set; }

		public PropertyInfo DataPropertyInfo { get; set; }

		public IValueConverter EditingValueConverter { get; set; }

		public Func<string, object> GetNodeFunc { private get; set; }

		public void RenderNode(object node) {
			if(node is TNode) {
				var typpedNode = (TNode)node;
				LambdaSetters.ForEach(a => a.Invoke(this, typpedNode));
			}
		}
		public void RenderNode(object node, string[] searchHighlightTexts) {
			RenderNode(node);
			if(SearchHighlight && !String.IsNullOrEmpty(Text) && searchHighlightTexts != null && searchHighlightTexts.Length > 0) {
				string resultMarkup = Text;
				foreach(var searchText in searchHighlightTexts) {
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
					if(DataPropertyInfo.PropertyType.IsAssignableFrom(typeof(TimeSpan?)))
						DataPropertyInfo.SetValue(node, null, null);
				}
				else {
					var len = new_text.Length;
					if(!new_text.Contains(":")) {
						if(len < 3) {
							new_text = $"{new_text}:00";
						}
						else {
							int.TryParse(new_text.Substring(0, 2), out int num);
							var pos = num > 23 && len == 3 ? 1 : 2;
							new_text = $"{new_text.Substring(0, pos)}:{new_text.Substring(pos, len - pos)}";
						}
					}
					if(TimeSpan.TryParse(new_text, out TimeSpan time))
						DataPropertyInfo.SetValue(node, time, null);
				}
			}
		}

		protected override void OnEditingStarted(CellEditable editable, string path) {
			if(editable is Entry entry) {
				entry.Changed += delegate (object sender, EventArgs args) {
					if(TimeSpan.TryParse(entry.Text, out TimeSpan outTime)) {
						entry.ModifyText(StateType.Normal);
					}
					else
						entry.ModifyText(StateType.Normal, new Color(255, 0, 0));
				};
			}
			base.OnEditingStarted(editable, path);
		}
	}
}

