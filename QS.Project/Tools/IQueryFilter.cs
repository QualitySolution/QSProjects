using System;
using QS.DomainModel.Entity;
using NHibernate.Criterion;
using QS.RepresentationModel;

namespace QS.Tools
{
	public interface IQueryFilter
	{
		ICriterion GetFilter();
	}

	public interface IQueryFilterView : IJournalFilter
	{
		IQueryFilter GetQueryFilter();
	}
}
