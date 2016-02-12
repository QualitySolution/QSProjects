using System;

namespace QSOrmProject.DomainMapping
{
	public interface ISearchProvider
	{
		bool Match(object entity, string searchText);
	}
}

