using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Project.Journal.Search.Criterion;

namespace QS.Project.Journal.EntitySelector
{
	public class DefaultEntityAutocompleteSelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel> : DefaultEntitySelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel> , IEntityAutocompleteSelectorFactory
		where TJournalViewModel : JournalViewModelBase, IEntityAutocompleteSelector
		where TJournalFilterViewModel : class, IJournalFilter
		where TEntity : class, IDomainObject
	{
		private readonly ICommonServices commonServices;
		private readonly ICriterionSearch criterionSearch;

		public DefaultEntityAutocompleteSelectorFactory(ICommonServices commonServices, ICriterionSearch criterionSearch) : base(commonServices, criterionSearch)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.criterionSearch = criterionSearch ?? throw new ArgumentNullException(nameof(criterionSearch));
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, UnitOfWorkFactory.GetDefaultFactory, commonServices, criterionSearch});
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
