using System;
using System.Collections.Generic;

namespace GammaBinding.ColumnConfig
{
	public interface IColumnsConfig
	{
		string GetColumnMappingString();

		IEnumerable<IColumnMapping> ConfiguredColumns { get;}
	}
}

