using System;
using System.Collections.Generic;

namespace Gamma.ColumnConfig
{
	public interface IColumnsConfig
	{
		string GetColumnMappingString();

		IEnumerable<IColumnMapping> ConfiguredColumns { get;}
	}
}

