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
		
		#region Between

		/// <summary>
		/// Between с использованием проекций для выражения и границ
		/// </summary>
		/// <param name="expression">Проекция выражения</param>
		/// <param name="min">Проекция левой границы(минимальное значение)</param>
		/// <param name="max">Проекция правой границы(максимальное значение)</param>
		/// <returns></returns>
		public static CustomBetweenExpression Between(IProjection expression, IProjection min, IProjection max)
			=> new CustomBetweenExpression(expression, min, max);
		
		/// <summary>
		/// Between с использованием проекций для выражения и границ
		/// </summary>
		/// <param name="expression">Проекция выражения</param>
		/// <param name="minName">Имя свойства класса для левой границы(минимальное значение)</param>
		/// <param name="maxName">Имя свойства класса для правой границы(максимальное значение)</param>
		/// <returns></returns>
		public static CustomBetweenExpression Between(IProjection expression, string minName, string maxName)
			=> new CustomBetweenExpression(expression, minName, maxName);
		
		/// <summary>
		/// Between с использованием проекций для выражения и границ
		/// </summary>
		/// <param name="expressionName">Имя свойства для выражения</param>
		/// <param name="minName">Имя свойства класса для левой границы(минимальное значение)</param>
		/// <param name="maxName">Имя свойства класса для правой границы(максимальное значение)</param>
		/// <returns></returns>
		public static CustomBetweenExpression Between(string expressionName, string minName, string maxName)
			=> new CustomBetweenExpression(expressionName, minName, maxName);

		#endregion
	}
}
