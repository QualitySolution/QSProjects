using System;
using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.Tdi;

namespace QS.Project.Journal.EntitySelector
{
	public class SimpleEntitySelectorFactory<TEntity, TEntityTab> : IEntityAutocompleteSelectorFactory
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TEntityTab : class, ITdiTab
	{
		private readonly Func<SimpleEntityJournalViewModel<TEntity, TEntityTab>> journalFunc;

		public SimpleEntitySelectorFactory(Func<SimpleEntityJournalViewModel<TEntity, TEntityTab>> journalFunc)
		{
			this.journalFunc = journalFunc ?? throw new ArgumentNullException(nameof(journalFunc));
		}

		public Type EntityType => typeof(TEntity);

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			return journalFunc.Invoke();
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			return CreateAutocompleteSelector(multipleSelect);
		}
	}
}
