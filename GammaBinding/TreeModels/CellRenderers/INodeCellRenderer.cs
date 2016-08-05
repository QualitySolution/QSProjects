using System;
using System.Reflection;
using Gamma.Binding;
using Gtk;

namespace Gamma.GtkWidgets.Cells
{

	public interface INodeCellRenderer
	{
		void RenderNode(object node);
		string DataPropertyName { get;}
		PropertyInfo DataPropertyInfo { get; }
		IValueConverter EditingValueConverter { get;}
	}

	public interface INodeCellRendererHighlighter : INodeCellRenderer
	{
		void RenderNode(object node, string[] searchHighlightText);
	}

	public interface INodeCellRendererCanGoNextCell : INodeCellRenderer
	{
		bool IsEnterToNextCell { get; }
		event EditingStartedHandler EditingStarted;
	}
}
