using System;
using QS.Dialog;
using QS.Updater.DTO;
using QSUpdater;

namespace QS.Updater
{
	public interface IUpdaterUI
	{
		IInteractiveMessage InteractiveMessage { get; }

		void ShowAppNewVersionDialog(UpdateResult result, UpdaterFlags flags);
	}
}
