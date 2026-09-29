using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader
{
	/// <param name="page">Номер запрашиваемой страницы, начиная с 1.</param>
	/// <param name="pageSize">Количество строк на странице.</param>
	public delegate IList<TNode> GetPageNodes<TNode>(int page, int pageSize, CancellationToken cancellation);

	/// <summary>
	/// Данный загрузчик позволяет журналу как угодно получать данные. Может из кода, может другим способом.
	/// Пока реализован только однопоточный режим.
	/// </summary>
	public class AnyDataLoader<TNode> : IDataLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly GetPageNodes<TNode> getPageNodes;
		private readonly Func<CancellationToken, int> getTotalCount;

		//Постраничный режим.
		private readonly bool pagedLoading;

		/// <summary>
		/// Загрузчик получающий все данные за раз.
		/// </summary>
		/// <param name="getNodes">Функция получения всех строк.</param>
		public AnyDataLoader(Func<CancellationToken, IList<TNode>> getNodes)
		{
			if(getNodes == null)
				throw new ArgumentNullException(nameof(getNodes));
			//Чтобы загрузка работала по одному алгоритму делаем частным случаем постраничной.
			getPageNodes = (page, pageSize, token) => getNodes(token);
			pagedLoading = false;
		}

		/// <summary>
		/// Загрузчик получающий данные постранично. 
		/// </summary>
		/// <param name="getPageNodes">Получение списка строк очередной страницы .</param>
		/// <param name="getTotalCount">Функция получения общего количества строк.</param>
		public AnyDataLoader(
			GetPageNodes<TNode> getPageNodes,
			Func<CancellationToken, int> getTotalCount = null)
		{
			this.getPageNodes = getPageNodes ?? throw new ArgumentNullException(nameof(getPageNodes));
			this.getTotalCount = getTotalCount;
			pagedLoading = true;
		}

		public IList Items { get; private set; } = new List<TNode>();

		public PostLoadProcessing PostLoadProcessingFunc { set => throw new NotImplementedException(); }
		public bool DynamicLoadingEnabled { get; set; } = true;
		public int PageSize { get; set; } = 100;
		public int? ItemsCountForNextLoad { get; set; }

		private bool hasUnloadedPages;
		public bool HasUnloadedItems => hasUnloadedPages;

		public bool FirstPage { get; private set; } = true;

		public bool TotalCountingInProgress { get; private set; }

		private uint? totalCount;
		public uint? TotalCount => getTotalCount == null ? (uint?)Items?.Count : totalCount;

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
			if(getTotalCount == null) {
				TotalCountingInProgress = true;
				if(Items == null)
					LoadData(false);
				TotalCountingInProgress = false;
				return;
			}

			if(totalCount.HasValue)
				return;

			if(cts.IsCancellationRequested)
				cts = new CancellationTokenSource();

			TotalCountingInProgress = true;
			Task.Factory.StartNew(() => totalCount = (uint)getTotalCount(cts.Token), cts.Token)
				.ContinueWith((tsk) => {
					TotalCountingInProgress = false;
					if(tsk.IsFaulted)
						OnLoadError(tsk.Exception);
					else
						TotalCountChanged?.Invoke(this, EventArgs.Empty);
				});
		}

		#region LoadingInPorogress

		int loadInProgress;
		public bool LoadInProgress => loadInProgress == 1;

		bool SetLoadInProgress(bool value) {
			int newValue = value ? 1 : 0;
			bool isSet = Interlocked.Exchange(ref loadInProgress, newValue) != newValue;
			if(isSet)
				OnLoadingStateChange(value ? LoadingState.InProgress : LoadingState.Idle);
			return isSet;
		}

		#endregion

		#region Загрузка данных
		private int reloadRequested = 0;
		private readonly List<TNode> loadedNodes = new List<TNode>();
		private int loadedPages;

		public void LoadData(bool nextPage)
		{
			if(cts.IsCancellationRequested)
				cts = new CancellationTokenSource();

			if (!SetLoadInProgress(true)) {
				Interlocked.Exchange(ref reloadRequested, 1);
				return;
			}

			FirstPage = !nextPage;
			DateTime startTime = DateTime.Now;
			Task.Factory.StartNew(() => LoadDataInternal(nextPage), cts.Token)
				.ContinueWith((tsk) => {
					SetLoadInProgress(false);
					if(tsk.IsFaulted)
						OnLoadError(tsk.Exception);
					logger.Info($"Загружено за {(DateTime.Now - startTime).TotalSeconds} сек.");
					ItemsListUpdated?.Invoke(this, EventArgs.Empty);
					TotalCountChanged?.Invoke(this, EventArgs.Empty);
				});
		}

		private void LoadDataInternal(bool nextPage)
		{
			if(!nextPage) {
				loadedNodes.Clear();
				loadedPages = 0;
				totalCount = null;
			}

			var needCount = !nextPage && ItemsCountForNextLoad.HasValue
				? Math.Max(ItemsCountForNextLoad.Value, PageSize)
				: loadedNodes.Count + PageSize;

			do {
				var page = getPageNodes(loadedPages + 1, PageSize, cts.Token);
				loadedPages++;
				loadedNodes.AddRange(page);
				//В полном режиме источник всегда отдает все за один вызов.
				//Неполная страница как признак конца списка.
				hasUnloadedPages = pagedLoading && page.Count == PageSize;
			} while(hasUnloadedPages && loadedNodes.Count < needCount && !cts.IsCancellationRequested);

			//Копируем чтобы снаружи всегда был доступен целый список, пока мы грузим следующее.
			Items = new List<TNode>(loadedNodes);
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
