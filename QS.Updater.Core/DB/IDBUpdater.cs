using System;
namespace QS.Updater.DB
{
	public interface IDBUpdater
	{
		bool HasUpdates { get; }
		void UpdateDB();
	}
}
