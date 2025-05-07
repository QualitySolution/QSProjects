using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.SqlCommand;

namespace QS.Project.DB.NhibernateExpressions {
	public class RlikeExpression : AbstractCriterion
	{
		private readonly string _propertyName;
		private readonly string _value;
		private readonly IProjection _projection;

		/// <summary>
		/// Initializes a new instance of the <see cref="RlikeExpression"/> class.
		/// </summary>
		/// <param name="projection">The projection.</param>
		/// <param name="value">The value.</param>
		public RlikeExpression(IProjection projection, string value)
		{
			_projection = projection;
			_value = value;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="RlikeExpression" /> 
		/// class for a named Property and its value.
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		public RlikeExpression(string propertyName, string value)
		{
			_propertyName = propertyName;
			_value = value;
		}

		public override SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			var sqlBuilder = new SqlStringBuilder();
			var columnNames =
				Utils.CriterionUtil.GetColumnNamesAsSqlStringParts(_propertyName, _projection, criteriaQuery, criteria);

			if(columnNames.Length != 1)
			{
				throw new HibernateException("rlike may only be used with single-column properties");
			}

			sqlBuilder.Add(criteriaQuery.Factory.Dialect.LowercaseFunction)
				.Add("(")
				.AddObject(columnNames[0])
				.Add(")")
				.Add(" rlike ");

			sqlBuilder.Add(criteriaQuery.NewQueryParameter(GetParameterTypedValue(criteria, criteriaQuery)).Single());

			return sqlBuilder.ToSqlString();
		}

		public override TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			var typedValues = new List<TypedValue>();

			if(_projection != null)
			{
				typedValues.AddRange(_projection.GetTypedValues(criteria, criteriaQuery));
			}
			typedValues.Add(GetParameterTypedValue(criteria, criteriaQuery));
			
			return typedValues.ToArray();
		}

		public TypedValue GetParameterTypedValue(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			var matchValue = _value.ToLower();
			return CriterionUtil.GetTypedValue(criteriaQuery, criteria, _projection, _propertyName, matchValue);
		}

		public override IProjection[] GetProjections()
		{
			if(_projection != null)
			{
				return new IProjection[] { _projection };
			}
			return null;
		}

		public override string ToString()
		{
			return (_projection ?? (object)_propertyName) + " rlike " + _value;
		}
	}
}
