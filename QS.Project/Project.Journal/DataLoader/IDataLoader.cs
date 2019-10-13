using System;
using System.Collections;

namespace QS.Project.Journal.DataLoader
{
	public interface IDataLoader
	{
		IList Items { get; }

		/// <summary>
		/// Список с данными был обновлен. Внимание, событие может приходить из другого потока.
		/// </summary>
		event EventHandler ItemsListUpdated;

		/// <summary>
		/// Общее количество строк получено. Внимание, событие может приходить из другого потока.
		/// </summary>
		event EventHandler TotalCountChanged;

		/// <summary>
		/// Вызывается при смене состояния загрузки. Может приходит из другого потока.
		/// </summary>
		event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;

		/// <summary>
		/// Вызывается если в загрузчике произошла ошибка. Может приходит из другого потока.
		/// </summary>
		event EventHandler<LoadErrorEventArgs> LoadError;

		bool DynamicLoadingEnabled { get; set; }

		bool HasUnloadedItems { get; }

		bool FirstPage { get; }

		bool LoadInProgress { get; }

		bool TotalCountingInProgress { get; }

		uint? TotalCount { get; }

		void GetTotalCount();

		void LoadData(bool nextPage);
	}

	public class LoadErrorEventArgs : EventArgs
	{
		public Exception Exception;
	}

	public class LoadingStateChangedEventArgs : EventArgs
	{
		public LoadingState LoadingState;
	}

	public enum LoadingState
	{
		Idle,
		InProgress,
	}
}
