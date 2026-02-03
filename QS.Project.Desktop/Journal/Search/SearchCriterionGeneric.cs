using System;
using System.Linq.Expressions;
using NHibernate.Criterion;

namespace QS.Journal.Search {
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
		
		/// <summary>
		/// Поиск в поле, с предварительной обработкой поисковой строки.
		/// </summary>
		/// <param name="prepareFunc">Функция возвращающая обработанную строку</param>
		/// <param name="aliases">Свойства по которым требует поиск</param>
		/// <returns></returns>
		public SearchCriterionGeneric<TEntity> ByPrepareValue(Func<string,string> prepareFunc, params Expression<Func<TEntity, object>>[] aliases) {
			foreach(var alias in aliases) {
				searchProperties.Add(SearchProperty.Create<TEntity>(alias, LikeMatchMode));
			}
			return this;
		}
		
		public new SearchCriterionGeneric<TEntity> ByPrepareValue(Func<string,string> prepareFunc, params Expression<Func<object>>[] aliases) {
			base.ByPrepareValue(prepareFunc, aliases);
			return this;
		}
		
		public new SearchCriterionGeneric<TEntity> WithLikeMode(MatchMode matchMode) {
			base.WithLikeMode(matchMode);
			return this;
		}

		#endregion
	}
}
