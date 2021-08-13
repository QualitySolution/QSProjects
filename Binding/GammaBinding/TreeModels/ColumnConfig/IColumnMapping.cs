using System;
using System.Collections.Generic;
using Gtk;

namespace Gamma.ColumnConfig
{
	public interface IColumnMapping
	{
		TreeViewColumn TreeViewColumn { get; }

		string Title { get;}

		bool IsEnterToNextCell { get; }

		IEnumerable<IRendererMapping> ConfiguredRenderers { get;}

		object tag { get; }
	}
}