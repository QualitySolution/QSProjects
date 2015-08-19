using System;
using System.Collections.Generic;

namespace GammaBinding.ColumnConfig
{
	public interface IColumnMapping
	{
		string Title { get;}

		bool IsEditable { get;}

		string DataPropertyName { get;}

		IEnumerable<IRendererMapping> ConfiguredRenderers { get;}
	}
}

