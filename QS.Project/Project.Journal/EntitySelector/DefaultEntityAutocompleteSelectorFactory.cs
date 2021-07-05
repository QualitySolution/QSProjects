using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;

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
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { });
			var journalActions = (EntitiesJournalActionsViewModel)JournalActionsConstructorInfo.Invoke(new object[] { });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(
				new object[] { journalActions, filter, UnitOfWorkFactory.GetDefaultFactory, commonServices });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
