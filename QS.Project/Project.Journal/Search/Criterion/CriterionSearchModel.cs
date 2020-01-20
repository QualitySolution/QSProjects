using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search.Criterion
{
	public class CriterionSearchModel : CriterionSearchModelBase
	{
		protected internal override ICriterion GetSearchCriterion()
		{
			Type[] digitsTypes = { typeof(decimal), typeof(int) };

			Conjunction conjunctionCriterion = new Conjunction();

			if(SearchValues == null || !SearchValues.Any()) {
				return conjunctionCriterion;
			}

			foreach(var sv in SearchValues) {
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}
				Disjunction disjunctionCriterion = new Disjunction();

				bool intParsed = int.TryParse(sv, out int intValue);
				bool decimalParsed = decimal.TryParse(sv, out decimal decimalValue);

				foreach(var aliasParameter in AliasParameters) {
					bool aliasIsInt = false;
					bool aliasIsDecimal = false;
					if(aliasParameter.Expression is UnaryExpression) {
						UnaryExpression unaryExpession = aliasParameter.Expression as UnaryExpression;
						aliasIsInt = unaryExpession.Operand.Type == typeof(int);
						aliasIsDecimal = unaryExpession.Operand.Type == typeof(decimal);
					} else if(!(aliasParameter.Expression is MemberExpression)) {
						throw new InvalidOperationException($"{nameof(aliasParameter)} должен быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
					}

					if(aliasIsInt) {
						if((intParsed)) {
							ICriterion restriction = Restrictions.Eq(aliasParameter.PropertyProjection, intValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else if(aliasIsDecimal) {
						if((decimalParsed)) {
							ICriterion restriction = Restrictions.Eq(aliasParameter.PropertyProjection, decimalValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else {
						var likeRestriction = Restrictions.Like(Projections.Cast(NHibernateUtil.String, aliasParameter.PropertyProjection), sv, MatchMode.Anywhere);
						disjunctionCriterion.Add(likeRestriction);
					}
				}
				conjunctionCriterion.Add(disjunctionCriterion);
			}

			return conjunctionCriterion;
		}
	}
}
