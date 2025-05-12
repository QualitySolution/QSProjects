using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace QS.Project.DB.Utils {
	public class CriterionUtil {
		public static object[] GetColumnNamesAsSqlStringParts(
			string propertyName,
			IProjection projection,
			ICriteriaQuery criteriaQuery,
			ICriteria criteria) {
			return propertyName != null
				? criteriaQuery.GetColumnsUsingProjection(criteria, propertyName)
				: GetColumnNamesAsSqlStringParts(projection, criteriaQuery, criteria);
		}
		
		public static object[] GetColumnNamesAsSqlStringParts(
			IProjection projection,
			ICriteriaQuery criteriaQuery,
			ICriteria criteria)
		{
			if (projection is IPropertyProjection propertyProjection)
			{
				return criteriaQuery.GetColumnsUsingProjection(criteria, propertyProjection.PropertyName);
			}

			return GetProjectionColumns(projection, criteriaQuery, criteria);
		}
		
		private static SqlString[] GetProjectionColumns(
			IProjection projection,
			ICriteriaQuery criteriaQuery,
			ICriteria criteria)
		{
			var sqlString = projection.ToSqlString(criteria, criteriaQuery.GetIndexForAlias(), criteriaQuery);
			return new[] { SqlStringHelper.RemoveAsAliasesFromSql(sqlString) };
		}
	}
}
