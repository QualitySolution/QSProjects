using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader
{
	/// <summary>
	/// Данный загрузчик позволяет журналу как угодно получать данные. Может из кода, может другим способом.
	/// Пока реализован только однопоточный режим.
	/// </summary>
	public class AnyDataLoader<TNode> : IDataLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly Func<CancellationToken, IList<TNode>> getNodes;

		public AnyDataLoader(Func<CancellationToken, IList<TNode>> getNodes)
		{
			this.getNodes = getNodes ?? throw new ArgumentNullException(nameof(getNodes));
		}

		public IList Items { get; private set; }

		public PostLoadProcessing PostLoadProcessingFunc { set => throw new NotImplementedException(); }
		public bool DynamicLoadingEnabled { get; set; } = true;
		public int PageSize { set => throw new NotImplementedException(); }
		public int? ItemsCountToNextLoad { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool HasUnloadedItems => false;

		public bool FirstPage => true;

		bool loadInProgress;
		public bool LoadInProgress {
			get => loadInProgress;
			private set {
				loadInProgress = value;
				OnLoadingStateChange(value ? LoadingState.InProgress : LoadingState.Idle);
			}
		}

		public bool TotalCountingInProgress { get; private set; }

		public uint? TotalCount => (uint?)Items?.Count;

		public event EventHandler ItemsListUpdated;
		public event EventHandler TotalCountChanged;
		public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;
		public event EventHandler<LoadErrorEventArgs> LoadError;

		private CancellationTokenSource cts = new CancellationTokenSource();
		public void CancelLoading()
		{
			cts.Cancel();
		}

		public IEnumerable<object> GetNodes(int entityId, IUnitOfWork uow)
		{
			throw new NotImplementedException();
		}

		public void GetTotalCount()
		{
			TotalCountingInProgress = true;
			if(Items == null)
				LoadData(false);
			TotalCountingInProgress = false;
		}

		#region Загрузка данных
		private int reloadRequested = 0;

		public void LoadData(bool nextPage)
		{
			if(cts.IsCancellationRequested)
				cts = new CancellationTokenSource();

			if(LoadInProgress) {
					Interlocked.Exchange(ref reloadRequested, 1);
				return;
			}

			LoadInProgress = true;
			DateTime startTime = DateTime.Now;
			Task.Factory.StartNew(() => Items = (IList)getNodes(cts.Token), cts.Token)
				.ContinueWith((tsk) => {
					LoadInProgress = false;
					if(tsk.IsFaulted)
						OnLoadError(tsk.Exception);
					logger.Info($"Загружено за {(DateTime.Now - startTime).TotalSeconds} сек.");
					ItemsListUpdated?.Invoke(this, EventArgs.Empty);
					TotalCountChanged?.Invoke(this, EventArgs.Empty);
				});
			Console.WriteLine("exit");
		}

		#endregion

		#region Вызов событий

		protected virtual void OnLoadError(Exception exception)
		{
			logger.Error(exception);
			var args = new LoadErrorEventArgs {
				Exception = exception
			};
			LoadError?.Invoke(this, args);
		}

		protected virtual void OnLoadingStateChange(LoadingState state)
		{
			var args = new LoadingStateChangedEventArgs {
				LoadingState = state
			};
			LoadingStateChanged?.Invoke(this, args);
		}

		#endregion
	}
}
