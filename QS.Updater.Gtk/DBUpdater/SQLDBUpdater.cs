using System;
namespace QS.Updater
{
	/// <summary>
	/// Пока просто класс адаптера, чтобы не рефакторить основной модуль который в этом нуждается.
	/// </summary>
	public class SQLDBUpdater : IDBUpdater
	{
		public void CheckMicroUpdatesDB()
		{
			DB.DBUpdater.CheckMicroUpdates();
		}

		public void CheckUpdateDB()
		{
			DB.DBUpdater.TryUpdate();
		}
	}
}
