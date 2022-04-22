using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.Project.Journal.DataLoader
{
	public class ThreadDataLoader<TNode> : IDataLoader
		where TNode: class
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Обязательные внешние зависимости

		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		#endregion

		public ThreadDataLoader(IUnitOfWorkFactory unitOfWorkFactory)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			cts = new CancellationTokenSource();
		}

		#region Опциональные внешние зависимости

		public ICurrentPermissionService CurrentPermissionService { get; set; }

		#endregion

		#region Events
		public event EventHandler ItemsListUpdated;
		public event EventHandler<LoadErrorEventArgs> LoadError;
		public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;
		public event EventHandler TotalCountChanged;

		public PostLoadProcessing PostLoadProcessingFunc { set; private get; }

		#endregion

		#region Настройка

		public bool ShowLateResults = true;

		#endregion

		#region Настройка работы

		public readonly IList<IQueryLoader<TNode>> QueryLoaders = new List<IQueryLoader<TNode>>();

		public readonly IList<SortRule<TNode>> OrderRules = new List<SortRule<TNode>>();

		public bool DynamicLoadingEnabled { get; set; } = true;

		public int PageSize { get; set; } = 100;

		#region Добавляем множенственны параметры

		/// <summary>
		/// Добавляем порядок сортировки всей коллекции. Эта настройка нужна только для загрузки с нескольких источников. Для загрузки из одного источника достаточно будет сортировки в самом запросе.
		/// Внимание! Чтобы слияние запросов проходило корректно, необходимо чтобы в каждом из запросов элементы были отсортированы в том же порядке.
		/// Можно вызывать несколько раз, для последовательной сортировки, сначала по одному значению потом по второму.
		/// </summary>
		/// <param name="orderByValueFunc">Функция получения значения для сортировки, в простом случае получения значения одного из полей.</param>
		/// <param name="descending">Если <c>true</c> сортировка в порядке убывания.</param>
		public void MergeInOrderBy(Func<TNode, object> orderByValueFunc, bool descending = false)
		{
			OrderRules.Add(new SortRule<TNode>(orderByValueFunc, descending));
		}

		/// <summary>
		/// Добавляем функцию получения запроса для загрузчика.
		/// Если функция возвращает null запрос не будет выполнятся. Это поведение используется в ситуации с несколькими запросами,
		/// в определенных обстоятельствах, например из-за параметров фильтра, некоторые запросы не имеет смысл выполнять.
		/// </summary>
		/// <param name="queryFunc">Функция формирования запроса.</param>
		/// <typeparam name="TRoot">Тип корня запроса</typeparam>
		public void AddQuery<TRoot>(Func<IUnitOfWork, IQueryOver<TRoot>> queryFunc)
			where TRoot : class , IDomainObject
		{
			QueryLoaders.Add(new DynamicQueryLoader<TRoot, TNode>((uow, isCounting) => queryFunc(uow), unitOfWorkFactory));
		}

		/// <summary>
		/// Добавляем функцию получения запроса для загрузчика.
		/// Если функция возвращает null запрос не будет выполнятся. Это поведение используется в ситуации с несколькими запросами,
		/// в определенных обстоятельствах, например из-за параметров фильтра, некоторые запросы не имеет смысл выполнять.
		/// </summary>
		/// <param name="queryFunc">Функция получения запроса, имеет параметры: 
		/// uow - для которого создается запрос 
		/// isCounting - указание является ли запрос подсчетом количества строк</param>
		/// <typeparam name="TRoot">Тип корня запроса</typeparam>
		public void AddQuery<TRoot>(Func<IUnitOfWork, bool, IQueryOver<TRoot>> queryFunc)
			where TRoot : class, IDomainObject
		{
			QueryLoaders.Add(new DynamicQueryLoader<TRoot, TNode>(queryFunc, unitOfWorkFactory));
		}

		#endregion
		#endregion

		#region Результат и состояние загрузки

		public IList Items {
			get {
				lock(publishedNodesLock) {
					return publishedNodes;
				}
			}
		}

		public bool HasUnloadedItems => AvailableQueryLoaders.Any(l => l.HasUnloadedItems);

		public bool FirstPage { get; private set; } = true;

		bool loadInProgress;
		public bool LoadInProgress { 
			get => loadInProgress;
			private set {
				loadInProgress = value;
				OnLoadingStateChange(value ? LoadingState.InProgress : LoadingState.Idle);
			} }

		public bool TotalCountingInProgress { get; private set; }

		public uint? TotalCount { get; private set; }

		#endregion

		#region Внутренние хелперы
		protected int? GetPageSize => DynamicLoadingEnabled ? PageSize : (int?)null;

		protected IEnumerable<IQueryLoader<TNode>> AvailableQueryLoaders {
			get { //Если сервис прав доступен, проверяем права. Все загрузчики запросов для которых у нас нет права чтения.
				if(CurrentPermissionService != null)
					return QueryLoaders.Where(x => !(x is IEntityQueryLoader) || CurrentPermissionService.ValidateEntityPermission((x as IEntityQueryLoader).EntityType).CanRead);
				else
					return QueryLoaders;
			}
		}

		#endregion

		#region Загрузка данных
		//Храним отдельно список узлов доступных снаружи и внутренний список с которым работает сам модуль.
		//Так как мы работает в отдельном потоке, снаружи всегда должен быть доступен предыдущий список. Пока мы обрабатываем новый запрос.
		private IList publishedNodes;
		private readonly List<TNode> readedNodes = new List<TNode>();
		private readonly object publishedNodesLock = new object();

		private int reloadRequested = 0;
		private Task[] RunningTasks = new Task[] { };
		private DateTime startLoading;

		private CancellationTokenSource cts;
		public void CancelLoading()
		{
			cts.Cancel();
		}

		public void LoadData(bool nextPage)
		{
			if(cts.IsCancellationRequested)
				cts = new CancellationTokenSource();

			Console.WriteLine($"LoadData({nextPage})");
			if (LoadInProgress) {
				if (!nextPage)
					Interlocked.Exchange(ref reloadRequested, 1);
				return;
			}

			LoadInProgress = true;
			LoadDataInternal(nextPage);
		}

		private void LoadDataInternal(bool nextPage)
		{
			startLoading = DateTime.Now;

			FirstPage = !nextPage;
			if (!nextPage) {
				readedNodes.Clear();
				TotalCount = null;
				TotalCountChanged?.Invoke(this, EventArgs.Empty);
				foreach (var item in QueryLoaders) {
					item.Reset();
				}
			}

			var runLoaders = AvailableQueryLoaders.Where(x => x.HasUnloadedItems).ToArray();

			if(runLoaders.Length == 0) { //Нет загрузчиков с помощь которых мы могли бы прочитать данные.
				LoadInProgress = false;
				return;
			}

			logger.Info("Запрос данных...");
			RunningTasks = new Task[runLoaders.Length];

			for (int i = 0; i < runLoaders.Length; i++) {
				var loader = runLoaders[i];
				RunningTasks[i] = Task.Factory.StartNew(() => loader.LoadPage(GetPageSize), cts.Token)
					.ContinueWith((tsk) => OnLoadError(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted);
			}

			Task.Factory.ContinueWhenAll(RunningTasks, (tasks) => {
				if(ShowLateResults || reloadRequested == 0) {
					if(AvailableQueryLoaders.Count() == 1)
						ReadOneLoader();
					else
						ReadLoadersInSortOrder();
					CopyNodesToPublish();
					if(cts.IsCancellationRequested) {
						logger.Info($"Загрузка данных отменена");
						return;
					}
					ItemsListUpdated?.Invoke(this, EventArgs.Empty);
				}
				logger.Info($"{(DateTime.Now - startLoading).TotalSeconds} сек.");
				if(1 == Interlocked.Exchange(ref reloadRequested, 0)) {
					LoadDataInternal(false);
				}
			}, cts.Token)
			.ContinueWith((tsk) => {
				LoadInProgress = false;
				if(tsk.IsFaulted)
					OnLoadError(tsk.Exception);
			});
		}

		#endregion

		#region Private

		private void CopyNodesToPublish()
		{
			var copied = readedNodes.ToList();
			lock(publishedNodesLock) {
				publishedNodes = copied;
			}
		}

		private void ReadOneLoader()
		{
			logger.Debug("Читаем данные одного загрузчика.");
			var beforeCount = readedNodes.Count;
			var loader = AvailableQueryLoaders.Cast<IPieceReader<TNode>>().First();
			readedNodes.AddRange(loader.TakeAllUnreadedNodes());
			if(readedNodes.Count > beforeCount)
				PostLoadProcessingFunc?.Invoke(readedNodes, (uint)beforeCount);
		}

		/// <summary>
		/// Метод поддерживает только лоадеры реализующие интерфейс IPieceReader
		/// </summary>
		private void ReadLoadersInSortOrder()
		{
			var beforeCount = readedNodes.Count;
			var loaders = AvailableQueryLoaders.Cast<IPieceReader<TNode>>().ToList();
			logger.Debug($"Обьединяем данные из {loaders.Count} загрузчиков.");
			var filtredLoaders = MakeOrderedEnumerable(loaders.Where(l => l.NextUnreadedNode() != null));
			while (loaders.Any(l => l.NextUnreadedNode() != null)) {
				if (loaders.Any(l => (l as IQueryLoader<TNode>).HasUnloadedItems && l.NextUnreadedNode() == null))
					break; //Уперлись в недогруженный хвост. Пока хватит, ждем следующей страницы.
				var taked = filtredLoaders.First();
				readedNodes.Add(taked.TakeNextUnreadedNode());
			}
			if(readedNodes.Count > beforeCount)
				PostLoadProcessingFunc?.Invoke(readedNodes, (uint)beforeCount);
		}

		private IEnumerable<IPieceReader<TNode>> MakeOrderedEnumerable(IEnumerable<IPieceReader<TNode>> loaders)
		{
			if (OrderRules != null && OrderRules.Any()) {
				IOrderedEnumerable<IPieceReader<TNode>> resultItems = null;
				bool isFirstRule = true;
				foreach (var orderRule in OrderRules) {
					if (isFirstRule) {
						if (orderRule.Descending)
							resultItems = loaders.OrderByDescending(l => orderRule.GetOrderByValue(l.NextUnreadedNode()));
						else
							resultItems = loaders.OrderBy(l => orderRule.GetOrderByValue(l.NextUnreadedNode()));
						isFirstRule = false;
					}
					else {
						if (orderRule.Descending)
							resultItems = resultItems.ThenByDescending(l => orderRule.GetOrderByValue(l.NextUnreadedNode()));
						else
							resultItems = resultItems.ThenBy(l => orderRule.GetOrderByValue(l.NextUnreadedNode()));
					}
				}
				return resultItems;
			}

			return loaders;
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

		#region Получение общего количества

		private Task[] CountingTasks = new Task[] { };
		private DateTime startCounting;
		private readonly object totalCountLock = new object();

		public void GetTotalCount()
		{
			if(TotalCountingInProgress) 
				return;

			if(TotalCount.HasValue)
				return; //Незачем пересчитывать.

			var runLoaders = AvailableQueryLoaders.ToArray();

			if(runLoaders.Length == 0) //Нет загрузчиков с помощь которых мы могли бы посчитать строки.
				return;

			if(cts.IsCancellationRequested)
				cts = new CancellationTokenSource();

			startCounting = DateTime.Now;
			TotalCountingInProgress = true;
			logger.Info("Запрос общего количества строк...");

			CountingTasks = new Task[runLoaders.Length];

			for(int i = 0; i < runLoaders.Length; i++) {
				var loader = runLoaders[i];
				CountingTasks[i] = Task.Factory.StartNew(() => {
					var countRows = loader.GetTotalItemsCount();
					Console.WriteLine(countRows);
					lock(totalCountLock) {
						TotalCount = (TotalCount ?? 0) + (uint)countRows;
					}
					if(cts.IsCancellationRequested) {
						logger.Info($"Загрузка общего количества строк отменена");
						return;
					}
					TotalCountChanged?.Invoke(this, EventArgs.Empty);
				}, cts.Token)
				.ContinueWith((tsk) => OnLoadError(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted);
			}

			Task.Factory.ContinueWhenAll(CountingTasks, (tasks) => {
				logger.Info($"{(DateTime.Now - startCounting).TotalSeconds} сек.");
				TotalCountingInProgress = false;
			});
		}

		public IEnumerable<object> GetNodes(int entityId, IUnitOfWork uow)
		{
			foreach(var item in QueryLoaders)
				yield return item.GetNode(entityId, uow);
		}

		#endregion
	}
}