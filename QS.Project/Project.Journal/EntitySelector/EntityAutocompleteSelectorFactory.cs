using System;
namespace QS.Project.Journal.EntitySelector
{
	public class EntityAutocompleteSelectorFactory<TJournal> : IEntityAutocompleteSelectorFactory
		where TJournal : JournalViewModelBase, IEntityAutocompleteSelector
	{
		private readonly Func<TJournal> selectorCtorFunc;

		public EntityAutocompleteSelectorFactory(Type entityType, Func<TJournal> selectorCtorFunc)
		{
			EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			this.selectorCtorFunc = selectorCtorFunc ?? throw new ArgumentNullException(nameof(selectorCtorFunc));
		}

		public Type EntityType { get; }

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var journal = selectorCtorFunc.Invoke();
			journal.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
			return journal;
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			return CreateAutocompleteSelector(multipleSelect);
		}
	}
}
