using System;
using System.Collections;

namespace QSOrmProject.DomainMapping
{
	public interface ISearchProvider
	{
		bool Match(object entity, string searchText);

		IList FilterList(IList sourcelist, string searchText);
	}
}

