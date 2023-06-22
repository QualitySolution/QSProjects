using System;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace QS.ViewModels.Control.EEVM
{
	public class JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel> : IEntityAutocompleteSelector<TEntity>, IDisposable
		where TEntity : class, IDomainObject
		where TJournalViewModel : JournalViewModelBase
	{
		protected readonly ILifetimeScope autofacScope;

		public JournalViewModelAutocompleteSelector(ILifetimeScope lifetimeScope)
		{
			this.autofacScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		protected TJournalViewModel journalViewModel;

		public event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;

		public virtual TJournalViewModel JournalViewModel {
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
			JournalViewModel.DataLoader.PageSize = takeCount;
			JournalViewModel.Search.SearchValues = searchText;
			JournalViewModel.Search.Update();
		}

		public string GetTitle(object node) => node.GetTitle();

		#region Внутреннее

		protected void DataLoader_ItemsListUpdated(object sender, EventArgs e)
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

	public class JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel, TJournalFilterViewModel> : JournalViewModelAutocompleteSelector<TEntity, TJournalViewModel>
		where TEntity : class, IDomainObject
		where TJournalViewModel : JournalViewModelBase
		where TJournalFilterViewModel : JournalFilterViewModelBase<TJournalFilterViewModel> {
		private readonly Action<TJournalFilterViewModel> filterParams;

		public JournalViewModelAutocompleteSelector(ILifetimeScope lifetimeScope, Action<TJournalFilterViewModel> filterParams):base(lifetimeScope) {
			this.filterParams = filterParams ?? throw new ArgumentNullException(nameof(filterParams));
		}

		public override TJournalViewModel JournalViewModel {
			get {
				if(journalViewModel == null) {
					journalViewModel = autofacScope.Resolve<TJournalViewModel>();
					if(journalViewModel.JournalFilter is JournalFilterViewModelBase<TJournalFilterViewModel> filter)
						filter.SetAndRefilterAtOnce(filterParams);
					else 
						throw new InvalidCastException($"Для установки параметров, фильтр {journalViewModel.JournalFilter.GetType()} должен является типом {typeof(JournalFilterViewModelBase<TJournalFilterViewModel>)}");
					journalViewModel.DataLoader.ItemsListUpdated += DataLoader_ItemsListUpdated;
				}
				return journalViewModel;
			}
		}
	}
}
