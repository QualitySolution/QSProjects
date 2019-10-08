using System;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.Services;

namespace QS.Project.Journal.EntitySelector
{
	public class DefaultEntityAutocompleteSelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel> : DefaultEntitySelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel> , IEntityAutocompleteSelectorFactory
		where TJournalViewModel : JournalViewModelBase, IEntityAutocompleteSelector
		where TJournalFilterViewModel : class, IJournalFilter
		where TEntity : class, IDomainObject
	{
		private readonly ICommonServices commonServices;

		public DefaultEntityAutocompleteSelectorFactory(ICommonServices commonServices) : base(commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { commonServices.InteractiveService });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, commonServices });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
