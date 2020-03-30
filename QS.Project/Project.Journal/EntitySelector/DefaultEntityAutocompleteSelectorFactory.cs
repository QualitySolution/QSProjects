using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Project.Journal.Search.Criterion;
using QS.Project.Journal.Search;

namespace QS.Project.Journal.EntitySelector
{
	public class DefaultEntityAutocompleteSelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel, TSearchModel> : DefaultEntitySelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel, TSearchModel> , IEntityAutocompleteSelectorFactory
		where TJournalViewModel : JournalViewModelBase, IEntityAutocompleteSelector
		where TJournalFilterViewModel : class, IJournalFilter
		where TEntity : class, IDomainObject
		where TSearchModel : CriterionSearchModelBase
	{
		private readonly ICommonServices commonServices;

		public DefaultEntityAutocompleteSelectorFactory(ICommonServices commonServices, SearchViewModelBase<TSearchModel> searchViewModel) : base(commonServices, searchViewModel)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, UnitOfWorkFactory.GetDefaultFactory, commonServices, SearchViewModel });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
