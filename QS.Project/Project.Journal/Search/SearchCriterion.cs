using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search {
	public class SearchCriterion {
		protected readonly IJournalSearch journalSearch;
		protected readonly List<SearchProperty> searchProperties = new List<SearchProperty>();

		public SearchCriterion(IJournalSearch journalSearch) {
			this.journalSearch = journalSearch ?? throw new ArgumentNullException(nameof(journalSearch));
		}

		#region Fluent

		public SearchCriterion By(params Expression<Func<object>>[] aliases) {
			foreach(var alias in aliases) {
				searchProperties.Add(SearchProperty.Create(alias));				
			}
			return this;
		}

		public ICriterion Finish() {
			Conjunction conjunctionCriterion = new Conjunction();

			if(journalSearch.SearchValues == null || !journalSearch.SearchValues.Any()) {
				return conjunctionCriterion;
			}

			foreach(var sv in journalSearch.SearchValues) {
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}

				Disjunction disjunctionCriterion = new Disjunction();
				foreach(var property in searchProperties) {
					var propertyCriterion = property.GetCriterion(sv);
					if(propertyCriterion != null) {
						disjunctionCriterion.Add(propertyCriterion);
					}
				}
				conjunctionCriterion.Add(disjunctionCriterion);
			}
			
			return conjunctionCriterion;
		} 
		#endregion
	}
}
