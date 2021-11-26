using System;
using System.Collections;
using System.Collections.Generic;
using QS.DomainModel.UoW;

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

		PostLoadProcessing PostLoadProcessingFunc { set; }

		bool DynamicLoadingEnabled { get; set; }

		int PageSize { set; }

		bool HasUnloadedItems { get; }

		bool FirstPage { get; }

		bool LoadInProgress { get; }

		bool TotalCountingInProgress { get; }

		uint? TotalCount { get; }

		void GetTotalCount();

		void LoadData(bool nextPage);

		IEnumerable<object> GetNodes(int entityId);

		void CancelLoading();
	}

	/// <summary>
	/// Делегат функции пост обработки списка.
	/// </summary>
	public delegate void PostLoadProcessing(IList items, uint addedSince);

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
