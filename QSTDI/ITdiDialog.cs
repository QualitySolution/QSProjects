using System;

namespace QSTDI
{
	public interface ITdiDialog : ITdiTab
	{
		bool HasChanges { get;}
		bool Save();
		event EventHandler<EntitySavedEventArgs> EntitySaved;
	}

	public class EntitySavedEventArgs : EventArgs
	{
		public object Entity { get; private set;}
		public bool TabClosed { get; private set;}

		public EntitySavedEventArgs(object entity, bool tabClosed = false)
		{
			Entity = entity;
			TabClosed = tabClosed;
		}
	}
}

