using NHibernate.Criterion;
using QS.Project.DB.NhibernateExpressions;

namespace QS.Project.DB {
	public static class CustomRestrictions {
		/// <summary>
		/// Для создания OR условий в Having, например sum(amount) > 0 or sum(discount) >0
		/// </summary>
		/// <param name="lhs">левая часть выражения</param>
		/// <param name="rhs">правая часть</param>
		/// <returns>выражение Having (lhs OR rhs)</returns>
		public static OrHavingExpression OrHaving(ICriterion lhs, ICriterion rhs) => new OrHavingExpression(lhs, rhs);

		#region RLIKE

		/// <summary>
		/// Для поиска, используя шаблон regex
		/// </summary>
		/// <param name="projection">Проекция</param>
		/// <param name="value">Шаблон поиска</param>
		/// <example>
		/// Поиск всех не числовых значений в свойстве комната точки доставки
		/// <c>query.And(CustomRestrictions.Rlike(Projections.Property(() => deliveryPointAlias.Room), "[^\\d]"))</c>
		/// </example>
		/// <returns></returns>
		public static RlikeExpression Rlike(IProjection projection, string value) => new RlikeExpression(projection, value);

		/// <summary>
		/// Для поиска, используя шаблон regex
		/// </summary>
		/// <param name="propertyName">Свойство</param>
		/// <param name="value">Шаблон поиска</param>
		/// <returns></returns>
		public static RlikeExpression Rlike(string propertyName, string value) => new RlikeExpression(propertyName, value);

		#endregion
	}
}
