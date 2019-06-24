using System;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search
{
	public interface IQuerySearch
	{
		ICriterion GetCriterionForSearch();
	}
}
