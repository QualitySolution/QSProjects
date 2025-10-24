using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.SqlCommand;

namespace QS.Project.DB.NhibernateExpressions {
	public class CustomBetweenExpression : AbstractCriterion {

		private const string _name = "between";
		private readonly string _expressionName;
		private readonly string _minName;
		private readonly string _maxName;
		private readonly IProjection _expression;
		private readonly IProjection _min;
		private readonly IProjection _max;
		
		public CustomBetweenExpression(string expressionName, string minName, string maxName) {
			_expressionName = expressionName;
			_minName = minName;
			_maxName = maxName;
		}
		
		public CustomBetweenExpression(IProjection expression, string minName, string maxName) {
			_expression = expression;
			_minName = minName;
			_maxName = maxName;
		}
		
		public CustomBetweenExpression(IProjection expression, IProjection min, IProjection max) {
			_expression = expression;
			_min = min;
			_max = max;
		}

		public override SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery) {
			var expressionNames =
				Utils.CriterionUtil.GetColumnNamesAsSqlStringParts(_expressionName, _expression, criteriaQuery, criteria);
			var minNames =
				Utils.CriterionUtil.GetColumnNamesAsSqlStringParts(_minName, _min, criteriaQuery, criteria);
			var maxNames =
				Utils.CriterionUtil.GetColumnNamesAsSqlStringParts(_maxName, _max, criteriaQuery, criteria);

			var sb = new SqlStringBuilder();
			
			if(expressionNames.Length == 1) {
				sb.AddObject(expressionNames[0])
					.Add($" {_name} ")
					.AddObject(minNames[0])
					.Add(" and ")
					.AddObject(maxNames[0]);
			}
			else {
				for(var i = 0; i < expressionNames.Length; i++)
				{
					if(i > 0)
					{
						sb.Add(" and ");
					}

					sb.AddObject(expressionNames[i])
						.Add(" >= ")
						.AddObject(minNames[i]);
				}

				for(var i = 0; i < expressionNames.Length; i++)
				{
					sb.Add(" and ")
						.AddObject(expressionNames[i])
						.Add(" <= ")
						.AddObject(maxNames[i]);
				}
			}
			
			return sb.ToSqlString();
		}

		public override TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery) {
			var types = new List<TypedValue>(CriterionUtil.GetTypedValues(criteriaQuery, criteria, _expression, _expressionName));
			types.AddRange(CriterionUtil.GetTypedValues(criteriaQuery, criteria, _min, _minName));
			types.AddRange(CriterionUtil.GetTypedValues(criteriaQuery, criteria, _max, _maxName));

			return types.Any() ? types.ToArray() : Array.Empty<TypedValue>();
		}

		public override IProjection[] GetProjections()
		{
			var projections = new List<IProjection>();
			
			if(_expression != null)
			{
				projections.Add(_expression);
			}
			
			if(_min != null)
			{
				projections.Add(_min);
			}
			
			if(_max != null)
			{
				projections.Add(_max);
			}
			
			return projections.Any() ? projections.ToArray() : null;
		}
		
		public override string ToString() {
			return (_expression ?? (object)_expressionName)
				+ $" {_name} "
				+ (_min ?? (object)_minName)
				+ " and "
				+ (_max ?? (object)_maxName);
		}
	}
}
