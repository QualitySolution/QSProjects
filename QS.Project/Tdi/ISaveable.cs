using System;

namespace QS.Tdi {
	public interface ISaveable {
		event EventHandler<EntitySavedEventArgs> EntitySaved;

		bool Save();
		void SaveAndClose();
	}
}