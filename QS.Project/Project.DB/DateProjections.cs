using System.Collections.Concurrent;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;

namespace QS.Project.DB {
	/// <summary>
	/// Проекции для работы с датой
	/// </summary>
	public static class DateProjections {
		private const string _dateAddFormat = "DATE_ADD(?1, INTERVAL ?2 {0})";
		private const string _dateSubFormat = "DATE_SUB(?1, INTERVAL ?2 {0})";
		
		private static readonly ConcurrentDictionary<string, ISQLFunction> _dateAddFunctionCache =
			new ConcurrentDictionary<string, ISQLFunction>();
		private static readonly ConcurrentDictionary<string, ISQLFunction> _dateSubFunctionCache =
			new ConcurrentDictionary<string, ISQLFunction>();
		
		/// <summary>
		/// Проекция sql функции DATE_ADD
		/// </summary>
		/// <param name="date">Проекция даты</param>
		/// <param name="interval">Проекция значения интервала</param>
		/// <param name="intervalType">Период(день, месяц, год и т.д.)</param>
		/// <returns></returns>
		public static IProjection DateAdd(IProjection date, IProjection interval, string intervalType) {
			var sqlFunction = GetDateDiffFunction(intervalType.ToUpper(), _dateAddFormat, _dateAddFunctionCache);
			return Projections.SqlFunction(sqlFunction, NHibernateUtil.DateTime, date, interval);
		}
		
		/// <summary>
		/// Проекция sql функции DATE_SUB
		/// </summary>
		/// <param name="date">Проекция даты</param>
		/// <param name="interval">Проекция значения интервала</param>
		/// <param name="intervalType">Период(день, месяц, год и т.д.)</param>
		public static IProjection DateSub(IProjection date, IProjection interval, string intervalType) {
			var sqlFunction = GetDateDiffFunction(intervalType.ToUpper(), _dateSubFormat, _dateSubFunctionCache);
			return Projections.SqlFunction(sqlFunction, NHibernateUtil.DateTime, date, interval);
		}
		
		private static ISQLFunction GetDateDiffFunction(
			string intervalType,
			string format,
			ConcurrentDictionary<string, ISQLFunction> cache)
		{
			if(!cache.TryGetValue(intervalType, out var sqlFunction))
			{
				var functionTemplate = string.Format(format, intervalType);
				sqlFunction = new SQLFunctionTemplate(NHibernateUtil.DateTime, functionTemplate);

				cache.TryAdd(intervalType, sqlFunction);
			}

			return sqlFunction;
		}
	}
}
