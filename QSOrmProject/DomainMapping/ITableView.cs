using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Binding;

namespace QSOrmProject.DomainMapping
{
	public interface ITableView
	{
		List<OrderByItem> OrderBy { get;}

		ISearchProvider SearchProvider { get;}

		IRecursiveTreeConfig RecursiveTreeConfig { get;}

		IColumnsConfig GetGammaColumnsConfig();
	}
}

