using System;
using System.ComponentModel;
using System.Linq.Expressions;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.Search.Criterion;

namespace QS.Project.Journal
{
	/*
	public class JournalQueryConfigurator<TEntity, TNode>
		where TEntity : class, INotifyPropertyChanged, IDomainObject, new()
		where TNode : JournalEntityNodeBase
	{
		private readonly JournalEntityConfigurator<TEntity, TNode> journalEntityConfigurator;
		private readonly QueryFactory<TEntity> queryFactory;

		public JournalQueryConfigurator(JournalEntityConfigurator<TEntity, TNode> journalEntityConfigurator, QueryFactory<TEntity> queryFactory)
		{
			this.journalEntityConfigurator = journalEntityConfigurator ?? throw new ArgumentNullException(nameof(journalEntityConfigurator));
			this.queryFactory = queryFactory ?? throw new ArgumentNullException(nameof(queryFactory));
		}

		public JournalQueryConfigurator<TEntity, TNode> AddSearchingBy(Expression<Func<object>> aliasExpression)
		{
			queryFactory.AddSearchingBy(aliasExpression);
			return this;
		}

		public JournalQueryConfigurator<TEntity, TNode> AddSearchingBy<TAlias>(Expression<Func<TAlias, object>> aliasExpression)
		{
			queryFactory.AddSearchingBy(aliasExpression);
			return this;
		}

		public JournalEntityConfigurator<TEntity, TNode> SetQuery(Func<IUnitOfWork, IQueryOver<TEntity>> queryFunction)
		{
			journalEntityConfigurator.SetQuery(queryFunction);
			return journalEntityConfigurator;
		}
	}*/
}
