using System;
using System.Reflection;
using Gamma.Binding;

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
}
