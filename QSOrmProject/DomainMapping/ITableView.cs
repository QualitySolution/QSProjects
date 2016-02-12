using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;

namespace QSOrmProject.DomainMapping
{
	public interface ITableView
	{
		List<OrderByItem> OrderBy { get;}

		ISearchProvider SearchProvider { get;}

		IColumnsConfig GetGammaColumnsConfig();
	}
}

