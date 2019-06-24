using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search
{
	public class QuerySearch : IQuerySearch
	{
		private readonly IJournalSearch search;

		public QuerySearch(IJournalSearch search)
		{
			this.search = search ?? throw new ArgumentNullException(nameof(search));
		}

		private Expression<Func<object>>[] aliasPropertiesToSearch;

		public void SetSearchProperties(Expression<Func<object>>[] aliasPropertiesToSearch)
		{
			this.aliasPropertiesToSearch = aliasPropertiesToSearch;
		}

		public ICriterion GetCriterionForSearch()
		{
			if(search == null)
				throw new ArgumentNullException(nameof(Search));

			ICriterion criterion = null;

			var searchString = search.SearchValues?.FirstOrDefault();
			if(string.IsNullOrWhiteSpace(searchString) || aliasPropertiesToSearch == null || !aliasPropertiesToSearch.Any())
				return criterion;

			foreach(var expr in aliasPropertiesToSearch) {
				var likeExpr = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(expr)), searchString, MatchMode.Anywhere);
				if(criterion == null)
					criterion = likeExpr;
				else
					criterion = Restrictions.Or(criterion, likeExpr);
			}

			return criterion;
		}
	}
}
