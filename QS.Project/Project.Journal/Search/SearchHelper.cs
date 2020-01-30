using System;
using System.Linq.Expressions;
using NHibernate.Criterion;
using System.Linq;
using NHibernate;

namespace QS.Project.Journal.Search
{
	public class SearchHelper
	{
		private readonly IJournalSearch journalSearch;

		public SearchHelper(IJournalSearch journalSearch)
		{
			this.journalSearch = journalSearch ?? throw new ArgumentNullException(nameof(journalSearch));
		}

		public ICriterion GetSearchCriterion<TEntity>(params Expression<Func<TEntity, object>>[] aliases)
		{
			Type[] digitsTypes = { typeof(decimal), typeof(int) };

			Conjunction conjunctionCriterion = new Conjunction();

			if(journalSearch.SearchValues == null || !journalSearch.SearchValues.Any()) {
				return conjunctionCriterion;
			}

			foreach(var sv in journalSearch.SearchValues) {
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}
				Disjunction disjunctionCriterion = new Disjunction();

				foreach(var alias in aliases) {
					Type typeOfPropery = null;
					if(alias.Body is UnaryExpression) {
						UnaryExpression unaryExpession = alias.Body as UnaryExpression;
						typeOfPropery = unaryExpession.Operand.Type;
					}
					else if(alias.Body is MemberExpression info)
						typeOfPropery = info.Type;
					else {
						throw new InvalidOperationException($"{nameof(alias)} должен быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
					}

					if (typeOfPropery == typeof(int)) {
						if(int.TryParse(sv, out int intValue)){
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), intValue);
							disjunctionCriterion.Add(restriction);
						}
					}
					else if (typeOfPropery == typeof(decimal)) {
						if(decimal.TryParse(sv, out decimal decimalValue)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), decimalValue);
							disjunctionCriterion.Add(restriction);
						}
					}
					else if (typeOfPropery == typeof(string)){
						var likeRestriction = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(alias)), sv, MatchMode.Anywhere);
						disjunctionCriterion.Add(likeRestriction);
					}
					else {
						throw new NotSupportedException($"Тип {typeOfPropery} не поддерживается");
					}

				}
				conjunctionCriterion.Add(disjunctionCriterion);
			}

			return conjunctionCriterion;
		}

		public ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliases)
		{
			Type[] digitsTypes = { typeof(decimal), typeof(int) };

			Conjunction conjunctionCriterion = new Conjunction();

			if(journalSearch.SearchValues == null || !journalSearch.SearchValues.Any()) {
				return conjunctionCriterion;
			}

			foreach(var sv in journalSearch.SearchValues) {
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}
				Disjunction disjunctionCriterion = new Disjunction();

				bool intParsed = int.TryParse(sv, out int intValue);
				bool decimalParsed = decimal.TryParse(sv, out decimal decimalValue);

				foreach(var alias in aliases) {
					bool aliasIsInt = false;
					bool aliasIsDecimal = false;
					if(alias.Body is UnaryExpression) {
						UnaryExpression unaryExpession = alias.Body as UnaryExpression;
						aliasIsInt = unaryExpession.Operand.Type == typeof(int);
						aliasIsDecimal = unaryExpession.Operand.Type == typeof(decimal);
					} else if(!(alias.Body is MemberExpression)) {
						throw new InvalidOperationException($"{nameof(alias)} должен быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
					}

					if(aliasIsInt) {
						if((intParsed)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), intValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else if(aliasIsDecimal) {
						if((decimalParsed)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), decimalValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else {
						var likeRestriction = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(alias)), sv, MatchMode.Anywhere);
						disjunctionCriterion.Add(likeRestriction);
					}
				}
				conjunctionCriterion.Add(disjunctionCriterion);
			}

			return conjunctionCriterion;
		}
	}
}
