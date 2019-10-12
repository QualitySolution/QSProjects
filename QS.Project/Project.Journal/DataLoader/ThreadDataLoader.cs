using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.Project.Journal.DataLoader
{
	public class ThreadDataLoader<TNode> : IDataLoader
		where TNode: class
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public ThreadDataLoader(IUnitOfWorkFactory unitOfWorkFactory)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		#region Обязательные внешние зависимости

		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		#endregion

		#region Опциональные внешние зависимости

		public ICurrentPermissionService CurrentPermissionService { get; set; }

		#endregion

		#region Events
		public event EventHandler ItemsListUpdated;

		public event EventHandler<LoadErrorEventArgs> LoadError;
		public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;

		#endregion

		#region Настройка работы

		public readonly List<IQueryLoader<TNode>> QueryLoaders = new List<IQueryLoader<TNode>>();

		public readonly List<SortRule<TNode>> OrderRules = new List<SortRule<TNode>>();

		public bool DynamicLoadingEnabled { get; set; } = true;

		public int PageSize { get; set; } = 100;

		#region Добавляем множенственны параметры

		/// <summary>
		/// Добавляем порядок сортировки всей коллекции. Эта настройка нужна только для загрузки с нескольких источников. Для загрузки из одного источкника достаточно будет сортировки в самом запросе.
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
		/// Добавляем запрос через загрузчик по умолчанию.
		/// </summary>
		/// <param name="queryFunc">Функция формирования запроса.</param>
		/// <typeparam name="TRoot">Тип корня запроса</typeparam>
		public void AddQuery<TRoot>(Func<IUnitOfWork, IQueryOver<TRoot>> queryFunc)
			where TRoot : class
		{
			QueryLoaders.Add(new DynamicQueryLoader<TRoot, TNode>(queryFunc, unitOfWorkFactory));
		}

		#endregion
		#endregion

		#region Результат и состояние загрузки

		public IList Items => nodes;

		public bool HasUnloadedItems => AvailableQueryLoaders.Any(l => l.HasUnloadedItems);

		public bool FirstPage { get; private set; } = true;

		public bool LoadInProgress { get; private set; }

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

		private List<TNode> nodes = new List<TNode>();
		private int reloadRequested = 0;
		private Task[] RunningTasks = new Task[] { };
		private DateTime startLoading;

		#region Methods

		public void LoadData(bool nextPage)
		{
			if (LoadInProgress) {
				if (!nextPage)
					Interlocked.Exchange(ref reloadRequested, 1);
				return;
			}

			startLoading = DateTime.Now;
			LoadInProgress = true;
			OnLoadingStateChange(LoadingState.InProgress);
			logger.Info("Запрос данных...");

			FirstPage = !nextPage;
			if (!nextPage) {
				nodes.Clear();
				foreach (var item in QueryLoaders) {
					item.Reset();
				}
			}

			var runLoaders = AvailableQueryLoaders.Where(x => x.HasUnloadedItems).ToArray();

			if(runLoaders.Length == 0) //Нет загрузчиков с помощь которы мы могли бы прочитать данные.
				return;

			RunningTasks = new Task[runLoaders.Length];

			for (int i = 0; i < runLoaders.Length; i++) {
				var loader = runLoaders[i];
				RunningTasks[i] = Task.Factory.StartNew(() => loader.LoadPage(GetPageSize))
					.ContinueWith((tsk) => OnLoadError(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted);
			}

			Task.Factory.ContinueWhenAll(RunningTasks, (tasks) => {
				if(AvailableQueryLoaders.Count() == 1)
					ReadOneLoader();
				else
					ReadLoadersInSortOrder();
				ItemsListUpdated?.Invoke(this, EventArgs.Empty);
				logger.Info($"{(DateTime.Now - startLoading).TotalSeconds} сек.");
				LoadInProgress = false;
				if(1 == Interlocked.Exchange(ref reloadRequested, 0)) {
					LoadData(false);
				} else
					OnLoadingStateChange(LoadingState.Idle);
			});
		}

		#endregion

		#region Private

		private void ReadOneLoader()
		{
			nodes = AvailableQueryLoaders.First().LoadedItems;
		}

		/// <summary>
		/// Метод поддерживает только лоадеры реализующие интерфейс IPieceReader
		/// </summary>
		private void ReadLoadersInSortOrder()
		{
			var loaders = AvailableQueryLoaders.Cast<IPieceReader<TNode>>().ToList();
			var filtredLoaders = MakeOrderedEnumerable(loaders.Where(l => l.NextUnreadedNode() != null));
			while (loaders.Any(l => l.NextUnreadedNode() != null)) {
				if (loaders.Any(l => (l as IQueryLoader<TNode>).HasUnloadedItems && l.NextUnreadedNode() == null))
					break; //Уперлись в неподгруженный хвост. Пока хватит, ждем следующей страницы.
				var taked = filtredLoaders.First();
				nodes.Add(taked.TakeNextUnreadedNode());
			}
		}

		private IEnumerable<IPieceReader<TNode>> MakeOrderedEnumerable(IEnumerable<IPieceReader<TNode>> loaders)
		{
			if (OrderRules != null && OrderRules.Any()) {
				IOrderedEnumerable<IPieceReader<TNode>> resultItems = null;
				bool isFirstValueInDictionary = true;
				foreach (var orderRule in OrderRules) {
					if (isFirstValueInDictionary) {
						if (orderRule.Descending)
							resultItems = loaders.OrderByDescending(l => orderRule.GetOrderByValue(l.NextUnreadedNode()));
						else
							resultItems = loaders.OrderBy(l => orderRule.GetOrderByValue(l.NextUnreadedNode()));
						isFirstValueInDictionary = false;
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

		protected virtual void OnLoadError(Exception exception)
		{
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
	}
}