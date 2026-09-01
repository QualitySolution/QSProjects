using System;
using System.Collections.Generic;
using Gtk;

namespace Gamma.ColumnConfig
{
	public interface IColumnMapping : IDisposable
	{
		TreeViewColumn TreeViewColumn { get; }

		string Title { get;}

		bool IsEnterToNextCell { get; }

		IEnumerable<IRendererMapping> ConfiguredRenderers { get;}

		object tag { get; }

		bool HasToolTip { get; }

		string GetTooltipText(object node);
	}
}
