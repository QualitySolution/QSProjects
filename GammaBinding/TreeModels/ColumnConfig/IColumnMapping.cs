using System;
using System.Collections.Generic;

namespace Gamma.ColumnConfig
{
	public interface IColumnMapping
	{
		string Title { get;}

		bool IsEditable { get;}

		float Alignment { get;}

		string DataPropertyName { get;}

		bool IsEnterToNextCell { get; }

		EventHandler ClickHandler { get; }

		IEnumerable<IRendererMapping> ConfiguredRenderers { get;}
	}
}