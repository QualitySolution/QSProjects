using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Project.Journal.DataLoader
{
	public class HierarchicalDataLoader<TEntity, TNode> : IHierarchicalDataLoader
		where TEntity : class, IDomainObject
		where TNode : class, IHierarchicalNode<TNode, TNode>
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		protected readonly IUnitOfWorkFactory _unitOfWorkFactory;
		protected Expression<Func<TEntity, object>> _parentEntityPropertyExpr;
		private Func<IUnitOfWork, IQueryOver<TEntity>> _queryFunc;
		private Func<IUnitOfWork, Func<IQueryOver<TEntity, TEntity>>, IQueryOver<TEntity, TEntity>> _mappingFunc;
		CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private bool _loadInProgress;

		public HierarchicalDataLoader(IUnitOfWorkFactory unitOfWorkFactory,
			Expression<Func<TEntity, object>> parentEntityPropertyExpr)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_parentEntityPropertyExpr = parentEntityPropertyExpr ?? throw new ArgumentNullException(nameof(parentEntityPropertyExpr));
		}

		public IList Items { get; private set; }

		private IQueryOver<TEntity> SetOrderBy(IQueryOver<TEntity, TEntity> queryOver)
		{
			return queryOver.OrderBy(Projections.Property(_parentEntityPropertyExpr)).Asc
				.OrderBy(Projections.Id()).Asc
				.TransformUsing(Transformers.AliasToBean<TNode>());
		}

		private IQueryOver<TEntity> AggregateQueryFunctions(IUnitOfWork unitOfWork)
        {
			return _mappingFunc(SetOrderBy(_queryFunc(unitOfWork) as IQueryOver<TEntity, TEntity>));
        }

		public PostLoadProcessing PostLoadProcessingFunc { set => throw new NotImplementedException(); }
		public bool DynamicLoadingEnabled { get; set; } = true;
		public int PageSize { set => throw new NotImplementedException(); }

		public bool HasUnloadedItems => false;

		public bool FirstPage => true;

		public bool TotalCountingInProgress { get; private set; }

		public uint? TotalCount { get; private set; }

		public event EventHandler ItemsListUpdated;
		public event EventHandler TotalCountChanged;
		public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;
		public event EventHandler<LoadErrorEventArgs> LoadError;

		public bool LoadInProgress
		{
			get => _loadInProgress;
			private set
			{
				_loadInProgress = value;
				OnLoadingStateChange(value ? LoadingState.InProgress : LoadingState.Idle);
			}
		}

		public void PreloadGrandchilds(object node) => PreloadGrandchilds((TNode)node);

		public void PreloadGrandchilds(TNode node)
		{

			//node.GetId();
		}

		public void CancelLoading()
		{
			_cancellationTokenSource.Cancel();
		}

		public IEnumerable<object> GetNodes(int entityId, IUnitOfWork uow)
		{
			throw new NotImplementedException();
		}

		public void GetTotalCount()
		{
			TotalCountingInProgress = true;
			if (Items == null)
			{
				LoadData();
			}
			TotalCountingInProgress = false;
		}

		private int reloadRequested = 0;

		/// <summary>
		/// Загрузка данных
		/// </summary>
		public void LoadData()
		{
			if (_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource = new CancellationTokenSource();
			}

			if (LoadInProgress)
			{
				Interlocked.Exchange(ref reloadRequested, 1);
				return;
			}

			LoadInProgress = true;
			DateTime startTime = DateTime.Now;
			Task.Factory.StartNew(() =>
				{
					var loadedItems = _aggregatedQueryFunc(_unitOfWorkFactory.CreateWithoutRoot()).List<TNode>();
					TotalCount = (uint?)loadedItems?.Count;
					Items = (IList)RearangeNodes(loadedItems);
				})
				.ContinueWith((tsk) =>
				{
					LoadInProgress = false;
					if (tsk.IsFaulted)
					{
						OnLoadError(tsk.Exception);
					}
					_logger.Info($"Загружено за {(DateTime.Now - startTime).TotalSeconds} сек.");
					ItemsListUpdated?.Invoke(this, EventArgs.Empty);
					TotalCountChanged?.Invoke(this, EventArgs.Empty);
				});
			Console.WriteLine("exit");
		}

		private IList<TNode> RearangeNodes(IList<TNode> sourceNodes, int? currentRootId = null)
		{
			var result = sourceNodes.Where(n => n.ParentId == currentRootId);
			TNode parentNode = null;

			if (currentRootId != null)
			{
				parentNode = sourceNodes.Where(n => n.GetId() == currentRootId).FirstOrDefault();
			}

			foreach (var node in result)
			{
				if (currentRootId != null)
				{
					node.Parent = parentNode;
				}

				if (node.Children == null)
				{
					node.Children = new List<TNode>();
				}

				(node.Children as List<TNode>).AddRange(RearangeNodes(sourceNodes, node.GetId()));
			}

			return result.ToList();
		}

		/// <summary>
		/// Добавление запроса по сущности с маппингом
		/// </summary>
		/// <param name="itemsSourceQueryFunction"></param>
		public void SetQueryFunc(Func<IUnitOfWork, IQueryOver<TEntity>> itemsSourceQueryFunction)
		{
			_queryFunc = itemsSourceQueryFunction;
		}

		public void SetMappingFunc(Func<IUnitOfWork, IQueryOver<TEntity>> mappingFunc)
        {
			_mappingFunc = mappingFunc;
        }

		protected virtual void OnLoadingStateChange(LoadingState state)
		{
			var args = new LoadingStateChangedEventArgs
			{
				LoadingState = state
			};
			LoadingStateChanged?.Invoke(this, args);
		}

		protected virtual void OnLoadError(Exception exception)
		{
			_logger.Error(exception);
			var args = new LoadErrorEventArgs
			{
				Exception = exception
			};
			LoadError?.Invoke(this, args);
		}
	}
}
