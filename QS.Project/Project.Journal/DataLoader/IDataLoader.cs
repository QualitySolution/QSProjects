using System;
using System.Collections;

namespace QS.Project.Journal.DataLoader
{
	public interface IDataLoader
	{
		IList Items { get; }

		event EventHandler ItemsListUpdated;

		event EventHandler<LoadErrorEventArgs> LoadError;

		bool DynamicLoadingEnabled { get; set; }

		bool HasUnloadedItems { get; }

		bool FirstPage { get; }

		bool LoadInProgress { get; }

		void LoadData(bool nextPage);
	}

	public class LoadErrorEventArgs : EventArgs
	{
		public Exception Exception;
	}
}
