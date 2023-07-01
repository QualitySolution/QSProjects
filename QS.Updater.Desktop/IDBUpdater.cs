using System;
namespace QS.Updater
{
	public interface IDBUpdater
	{
		bool HasUpdates { get; }
		void UpdateDB();
	}
}
