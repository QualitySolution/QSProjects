using System.Linq;
using NHibernate.Criterion;

namespace QS.Project.DB {
	public static class CustomRestrictions {
		/// <summary>
		/// Для создания OR условий в Having, например sum(amount) > 0 or sum(discount) >0
		/// </summary>
		/// <param name="lhs">левая часть выражения</param>
		/// <param name="rhs">правая часть</param>
		/// <returns>выражение Having (lhs OR rhs)</returns>
		public static OrHaving OrHaving(ICriterion lhs, ICriterion rhs) {
			return new OrHaving(lhs, rhs);
		}
	}

	public class OrHaving : OrExpression {
		public OrHaving(ICriterion lhs, ICriterion rhs) : base(lhs, rhs) { }

		public override IProjection[] GetProjections()
		{
			return LeftHandSide.GetProjections().Concat(RightHandSide.GetProjections()).ToArray();
		}
	}
}
