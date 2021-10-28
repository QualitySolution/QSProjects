using System;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;

namespace QS.ViewModels.Control.EEVM
{
	public class JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel> : IEntityAutocompleteSelector<TEntity>, IDisposable
		where TEntity : class, IDomainObject
		where TJournalViewModel : JournalViewModelBase
	{
		private readonly IUnitOfWork uow;
		private readonly ILifetimeScope autofacScope;

		public JournalViewModelAutocompleteSelector(IUnitOfWork unitOfWork, ILifetimeScope lifetimeScope)
		{
			this.uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			this.autofacScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		TJournalViewModel journalViewModel;

		public event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;

		public TJournalViewModel JournalViewModel {
			get {
				if (journalViewModel == null) {
					journalViewModel = autofacScope.Resolve<TJournalViewModel>();
					journalViewModel.DataLoader.ItemsListUpdated += DataLoader_ItemsListUpdated;
				}
				return journalViewModel;
			}
		}

		public void LoadAutocompletion(string[] searchText, int takeCount)
		{
			(JournalViewModel.DataLoader as IListLoader).PageSize = takeCount;
			JournalViewModel.Search.SearchValues = searchText;
			JournalViewModel.Search.Update();
		}

		public string GetTitle(object node)
		{
			return DomainHelper.GetTitle(node);
		}

		#region Внутреннее

		void DataLoader_ItemsListUpdated(object sender, EventArgs e)
		{
			AutocompleteLoaded?.Invoke(this, new AutocompleteUpdatedEventArgs(journalViewModel.Items));
		}

		#endregion

		public void Dispose()
		{
			if (journalViewModel != null)
				journalViewModel.Dispose();
		}
	}
}
