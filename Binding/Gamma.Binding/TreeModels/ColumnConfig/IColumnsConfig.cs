using System;
using System.Collections;
using System.Collections.Generic;
using Gamma.Binding;
using Gtk;

namespace Gamma.ColumnConfig
{
	public interface IColumnsConfig
	{
		IEnumerable<IColumnMapping> ConfiguredColumns { get;}
		Func<IyTreeModel> TreeModelFunc { get; }
		IEnumerable<IRendererMapping> GetRendererMappingByTag(object tag);
		IEnumerable<TRendererMapping> GetRendererMappingByTagGeneric<TRendererMapping>(object tag);
		IEnumerable<TreeViewColumn> GetColumnsByTag(object tag);
	}
}

