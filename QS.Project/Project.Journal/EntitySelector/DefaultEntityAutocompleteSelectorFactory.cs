using System;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.Project.Journal.EntitySelector
{
	public class DefaultEntityAutocompleteSelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel> : DefaultEntitySelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel> , IEntityAutocompleteSelectorFactory
		where TJournalViewModel : JournalViewModelBase, IEntityAutocompleteSelector
		where TJournalFilterViewModel : class, IJournalFilter
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWorkFactory uowFactory;
		private readonly ICommonServices commonServices;

		public DefaultEntityAutocompleteSelectorFactory(IUnitOfWorkFactory uowFactory, ICommonServices commonServices) : base(uowFactory, commonServices)
		{
			this.uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, uowFactory, commonServices });
			selectorViewModel.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
