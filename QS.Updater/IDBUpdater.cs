using System;
namespace QS.Updater
{
	public interface IDBUpdater
	{
		void CheckUpdateDB();
		void CheckMicroUpdatesDB();
	}
}
