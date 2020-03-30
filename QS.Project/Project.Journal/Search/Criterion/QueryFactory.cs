using System;
using System.Linq.Expressions;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.Search.Criterion
{
	//public class QueryFactory<TEntity>
	//	where TEntity : class, IDomainObject
	//{
	//	private readonly CriterionSearchModelBase criterionSearchModel;

	//	internal Func<IUnitOfWork, IQueryOver<TEntity>> QueryFunc { get; private set; }

	//	public QueryFactory(CriterionSearchModelBase criterionSearchModel)
	//	{
	//		this.criterionSearchModel = criterionSearchModel ?? throw new ArgumentNullException(nameof(criterionSearchModel));
	//	}

	//	public void AddSearchingBy(Expression<Func<object>> aliasExpression)
	//	{
	//		criterionSearchModel.AddSearchBy(aliasExpression);
	//	}

	//	public void AddSearchingBy<TAlias>(Expression<Func<TAlias, object>> aliasExpression)
	//	{
	//		criterionSearchModel.AddSearchBy(aliasExpression);
	//	}

	//	public void SetQuery(Func<IUnitOfWork, IQueryOver<TEntity>> queryFunc)
	//	{
	//		QueryFunc = queryFunc;
	//	}
	//}
}
