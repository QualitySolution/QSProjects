using System;
using QS.DomainModel.Entity;

namespace QS.Tdi
{
	public interface ITdiDialog : ITdiTab
	{
		bool HasChanges { get; }
		bool Save();
		void SaveAndClose();
		event EventHandler<EntitySavedEventArgs> EntitySaved;
	}

	public class EntitySavedEventArgs : EventArgs
	{
		public object Entity { get; private set; }
		public bool TabClosed { get; private set; }

		public EntitySavedEventArgs(object entity, bool tabClosed = false)
		{
			Entity = entity;
			TabClosed = tabClosed;
		}

		public T GetEntity<T>() where T : class, IDomainObject => Entity as T;
	}
}

