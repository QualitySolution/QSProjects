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
			popupWindow?.Destroy();
			popupWindow = null;
			
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

		private Gtk.Window popupWindow;
		private Calendar popupCalendar;
		
		public override CellEditable StartEditing(Event evnt, Widget widget, string path, Rectangle background_area, Rectangle cell_area, CellRendererState flags) {
			popupWindow = new Gtk.Window(Gtk.WindowType.Popup);
			popupCalendar = new Calendar();
			popupCalendar.ShowAll();
			popupWindow.Add(popupCalendar);
			popupWindow.ShowAll();
			SetPosition((TreeView)widget, popupWindow, background_area, CalendarXAlignment, CalendarYAllocation);
			
			return base.StartEditing(evnt, widget, path, background_area, cell_area, flags);
		}

		protected override void OnEditingCanceled() {
			popupWindow?.Destroy();
			popupWindow = null;
			base.OnEditingCanceled();
		}

		protected override void OnEditingStarted(CellEditable editable, string path) {
			if(editable is Entry entry) {
				entry.Changed += delegate(object sender, EventArgs args) {
					if(DateTime.TryParse(entry.Text, out DateTime outDate)) {
						entry.ModifyText(StateType.Normal);
						if(popupCalendar?.Date != outDate)
							popupCalendar.Date = outDate;
					}
					else
						entry.ModifyText(StateType.Normal, new Color(255,0,0)); 
				};
				if(popupCalendar != null)
					popupCalendar.DaySelected += (sender, args) => entry.Text = popupCalendar.Date.ToShortDateString();
			}
			base.OnEditingStarted(editable, path);
		}
		
		public virtual HorizontalAlignment CalendarXAlignment { get; set; } = HorizontalAlignment.Auto;
		public virtual VerticalAllocation CalendarYAllocation { get; set; } = VerticalAllocation.Auto;
		
		public static void SetPosition(TreeView treeView, Gtk.Window popup, Rectangle boundRectangle, HorizontalAlignment horizontal, VerticalAllocation vertical)
		{
			int x, y;
			int widgetX, widgetY;
			int maxX, maxY;
			treeView.ConvertWidgetToTreeCoords(treeView.GdkWindow.Screen.Width, treeView.GdkWindow.Screen.Height, out maxX, out maxY);
			treeView.GdkWindow.GetOrigin(out widgetX, out widgetY);
			switch(vertical) {
				case VerticalAllocation.Top:
					y = boundRectangle.Top + widgetY - popup.Allocation.Height;
					break;
				case VerticalAllocation.Bottom:
					y = boundRectangle.Bottom + widgetY;
					break;
				case VerticalAllocation.Auto:
				default:
					y = boundRectangle.Bottom + widgetY;
					if(maxY < y + popup.Allocation.Height)
						y = widgetY + boundRectangle.Top - popup.Allocation.Height;
					break;
			}
		
			switch(horizontal) {
				case HorizontalAlignment.Left:
					x = boundRectangle.X + widgetX;
					break;
				case HorizontalAlignment.Right:
					x = boundRectangle.X + widgetX + boundRectangle.Width - popup.Allocation.Width;
					break;
				case HorizontalAlignment.Auto:
				default:
					x = boundRectangle.X + widgetX;
					if(maxX < x + popup.Allocation.Width)
						x = widgetX + boundRectangle.Right - popup.Allocation.Width;
					break;
			}
			treeView.ConvertTreeToWidgetCoords(x,y, out x, out y);
			popup.Move(x, y);
		}
	}

	public enum VerticalAllocation
	{
		Auto,
		Top,
		Bottom
	}

	public enum HorizontalAlignment
	{
		Auto,
		Left,
		Right
	}
}

