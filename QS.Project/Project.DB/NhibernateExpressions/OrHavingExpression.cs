using System.Linq;
using NHibernate.Criterion;

namespace QS.Project.DB.NhibernateExpressions
{
	public class OrHavingExpression : OrExpression {
		public OrHavingExpression(ICriterion lhs, ICriterion rhs) : base(lhs, rhs) { }

		public override IProjection[] GetProjections()
		{
			return LeftHandSide.GetProjections().Concat(RightHandSide.GetProjections()).ToArray();
		}
	}
}
