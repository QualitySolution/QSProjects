using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search.Criterion
{
	/// <summary>
	/// Предоставляет инструмент настройки алиасов, с помощью Fluent интерфейса.
	/// Алиасы могут быть использованы для построения поискового ICriterion при 
	/// реализации метода GetSearchCriterion()
	/// </summary>
	public abstract class CriterionSearchModelBase : SearchModel
	{
		internal CriterionSearchModelConfig CriterionSearchConfig { get; private set; }

		protected IEnumerable<SearchAliasParameter> AliasParameters => CriterionSearchConfig.AliasParameters;

		protected CriterionSearchModelBase()
		{
		}

		/// <summary>
		/// Создает конфигуратор алиасов
		/// </summary>
		public CriterionSearchModelConfig ConfigureSearch()
		{
			CriterionSearchConfig = new CriterionSearchModelConfig(this);
			return CriterionSearchConfig;
		}

		/// <summary>
		/// Предоставляет поисковый ICriterion.
		/// При формировании ICriterion для доступа к настроенным алиасам 
		/// необходимо обращаться к AliasParameters свойству
		/// </summary>
		protected internal abstract ICriterion GetSearchCriterion();
	}

	public class SearchAliasParameter
	{
		public System.Linq.Expressions.Expression Expression { get; }
		public PropertyProjection PropertyProjection { get; }

		internal SearchAliasParameter(System.Linq.Expressions.Expression expression, PropertyProjection propertyProjection)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			PropertyProjection = propertyProjection ?? throw new ArgumentNullException(nameof(propertyProjection));
		}
	}

	public class CriterionSearchModelConfig
	{
		private readonly CriterionSearchModelBase criterionSearchModel;
		internal List<SearchAliasParameter> AliasParameters { get; } = new List<SearchAliasParameter>();
		private CriterionSearchModelAliasConfig aliasConfig;

		internal CriterionSearchModelConfig(CriterionSearchModelBase criterionSearchModel)
		{
			this.criterionSearchModel = criterionSearchModel ?? throw new ArgumentNullException(nameof(criterionSearchModel));
			aliasConfig = new CriterionSearchModelAliasConfig(criterionSearchModel, this);
		}

		public CriterionSearchModelAliasConfig AddSearchBy(params Expression<Func<object>>[] aliases)
		{
			foreach(var alias in aliases) {
				AliasParameters.Add(new SearchAliasParameter(alias, Projections.Property(alias)));
			}

			return aliasConfig;
		}

		public CriterionSearchModelAliasConfig AddSearchBy<TEntity>(params Expression<Func<TEntity, object>>[] aliases)
		{
			foreach(var alias in aliases) {
				AliasParameters.Add(new SearchAliasParameter(alias, Projections.Property(alias)));
			}

			return aliasConfig;
		}
	}

	public class CriterionSearchModelAliasConfig
	{
		private readonly CriterionSearchModelBase criterionSearchModel;
		private readonly CriterionSearchModelConfig searchConfig;

		internal CriterionSearchModelAliasConfig(CriterionSearchModelBase criterionSearchModel, CriterionSearchModelConfig searchConfig)
		{
			this.criterionSearchModel = criterionSearchModel ?? throw new ArgumentNullException(nameof(criterionSearchModel));
			this.searchConfig = searchConfig ?? throw new ArgumentNullException(nameof(searchConfig));
		}

		public CriterionSearchModelAliasConfig AddSearchBy(params Expression<Func<object>>[] aliases)
		{
			return searchConfig.AddSearchBy(aliases);

		}

		public CriterionSearchModelAliasConfig AddSearchBy<TEntity>(params Expression<Func<TEntity, object>>[] aliases)
		{
			return searchConfig.AddSearchBy(aliases);
		}

		public ICriterion GetSearchCriterion()
		{
			return criterionSearchModel.GetSearchCriterion();
		}
	}
}
