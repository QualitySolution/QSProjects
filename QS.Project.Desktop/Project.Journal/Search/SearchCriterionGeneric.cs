using System;
using System.Linq.Expressions;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search {
	public class SearchCriterionGeneric<TEntity> : SearchCriterion {
		public SearchCriterionGeneric(IJournalSearch journalSearch) : base(journalSearch)
		{
		}

		#region Fluent

		public SearchCriterionGeneric<TEntity> By(params Expression<Func<TEntity, object>>[] aliases) {
			foreach(var alias in aliases) {
				searchProperties.Add(SearchProperty.Create<TEntity>(alias, LikeMatchMode));
			}
			return this;
		}

		public new SearchCriterionGeneric<TEntity> By(params Expression<Func<object>>[] aliases) {
			base.By(aliases);
			return this;
		}
		
		public new SearchCriterionGeneric<TEntity> WithLikeMode(MatchMode matchMode) {
			base.WithLikeMode(matchMode);
			return this;
		}

		#endregion
	}
}
