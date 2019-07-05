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

		private IProjection[] propertiesToSearch;

		public void SetSearchProperties<TEntity>(Expression<Func<TEntity, object>>[] aliasPropertiesToSearch)
		{
			propertiesToSearch = aliasPropertiesToSearch.Select(x => Projections.Property(x)).ToArray();
		}

		public ICriterion GetCriterionForSearch()
		{
			if(search == null)
				throw new ArgumentNullException(nameof(Search));

			ICriterion criterion = null;

			var searchString = search.SearchValues?.FirstOrDefault();
			if(string.IsNullOrWhiteSpace(searchString))
				return criterion;
			
			if(aliasPropertiesToSearch != null) {
				foreach(var expr in aliasPropertiesToSearch) {
					var likeExpr = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(expr)), searchString, MatchMode.Anywhere);
					if(criterion == null)
						criterion = likeExpr;
					else
						criterion = Restrictions.Or(criterion, likeExpr);
				}
			}

			if(propertiesToSearch != null) {
				foreach(var propertyProjection in propertiesToSearch) {
					var likeExpr = Restrictions.Like(Projections.Cast(NHibernateUtil.String, propertyProjection), searchString, MatchMode.Anywhere);
					if(criterion == null)
						criterion = likeExpr;
					else
						criterion = Restrictions.Or(criterion, likeExpr);
				}
			}

			return criterion;
		}
	}
}
