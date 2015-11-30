using System;
using System.Collections.Generic;

namespace QSOrmProject.DomainMapping
{
	public interface ITableView
	{
		List<OrderByItem> OrderBy { get;}
		List<string> SearchBy { get;}
	}
}

