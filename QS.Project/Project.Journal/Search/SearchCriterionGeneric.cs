using System;
using System.Linq.Expressions;

namespace QS.Project.Journal.Search {
	public class SearchCriterionGeneric<TEntity> : SearchCriterion {
		public SearchCriterionGeneric(IJournalSearch journalSearch) : base(journalSearch)
		{
		}

		#region Fluent

		public SearchCriterion By<TEntity>(params Expression<Func<TEntity, object>>[] aliases) {
			foreach(var alias in aliases) {
				searchProperties.Add(SearchProperty.Create<TEntity>(alias));
			}
			return this;
		}

		#endregion
	}
}
