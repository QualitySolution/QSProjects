using System;
using System.Collections.Generic;

namespace Gamma.ColumnConfig
{
	public interface IColumnsConfig
	{
		IEnumerable<IColumnMapping> ConfiguredColumns { get;}
	}
}

