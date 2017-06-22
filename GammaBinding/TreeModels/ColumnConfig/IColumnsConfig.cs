using System;
using System.Collections.Generic;

namespace Gamma.ColumnConfig
{
	public interface IColumnsConfig
	{
		IEnumerable<IColumnMapping> ConfiguredColumns { get;}

		IEnumerable<IRendererMapping> GetRendererMappingByTag(object tag);
		IEnumerable<TRendererMapping> GetRendererMappingByTagGeneric<TRendererMapping>(object tag);
	}
}

