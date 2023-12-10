using System;
namespace QS.Project.Journal.EntitySelector
{
	public class EntityAutocompleteSelectorFactory<TJournal> : IEntityAutocompleteSelectorFactory
		where TJournal : JournalViewModelBase, IEntityAutocompleteSelector
	{
		private Func<TJournal> _selectorCtorFunc;

		public EntityAutocompleteSelectorFactory(Type entityType, Func<TJournal> selectorCtorFunc)
		{
			EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			_selectorCtorFunc = selectorCtorFunc ?? throw new ArgumentNullException(nameof(selectorCtorFunc));
		}

		public Type EntityType { get; }

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var journal = _selectorCtorFunc.Invoke();
			journal.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
			return journal;
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			return CreateAutocompleteSelector(multipleSelect);
		}

		public void Dispose() {
			_selectorCtorFunc = null;
		}
	}
}
