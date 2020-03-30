using System;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;

namespace QS.Project.Journal.EntitySelector
{
	public class DefaultEntitySelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel, TSearchModel> : IEntitySelectorFactory
		where TJournalViewModel : JournalViewModelBase, IEntitySelector
		where TJournalFilterViewModel : class, IJournalFilter
		where TEntity : class, IDomainObject
		where TSearchModel : CriterionSearchModelBase
	{
		private readonly ICommonServices commonServices;
		protected SearchViewModelBase<TSearchModel> SearchViewModel { get; }
		protected ConstructorInfo journalConstructorInfo;
		protected ConstructorInfo filterConstructorInfo;

		public Type EntityType => typeof(TEntity);

		public DefaultEntitySelectorFactory(ICommonServices commonServices, SearchViewModelBase<TSearchModel> searchViewModel)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			SearchViewModel = searchViewModel ?? throw new ArgumentNullException(nameof(searchViewModel));

			Type filterType = typeof(TJournalFilterViewModel);
			filterConstructorInfo = filterType.GetConstructor(new Type[] {});
			if(filterConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для фильтра {filterType.Name}");
			}

			Type journalType = typeof(TJournalViewModel);
			journalConstructorInfo = journalType.GetConstructor(new Type[] { filterType, typeof(IUnitOfWorkFactory), typeof(ICommonServices), typeof(SearchViewModelBase<TSearchModel>) });
			if(journalConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для журнала {journalType.Name} с параметрами:" +
					$"{filterType.Name}, {nameof(IUnitOfWorkFactory)},  {nameof(ICommonServices)}, {nameof(SearchViewModelBase<TSearchModel>)}");
			}
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, UnitOfWorkFactory.GetDefaultFactory, commonServices, SearchViewModel });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
