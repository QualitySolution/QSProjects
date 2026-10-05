using QS.DomainModel.UoW;
using System;
using System.Collections;
using System.Collections.Generic;

namespace QS.Project.Journal.DataLoader {
	public class ThreadSqlDataLoader<TNode> : IDataLoader
		where TNode : class 
	{
		ThreadDataLoader<TNode> _threadDataLoader;

		public ThreadSqlDataLoader(
			IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, int?, int?, IList<TNode>> limitedItemsFunction,
			Func<IUnitOfWork, int> itemsCountFunction
		) {
			_threadDataLoader = new ThreadDataLoader<TNode>(unitOfWorkFactory);
			_threadDataLoader.QueryLoaders.Add(new SqlQueryLoader<TNode>(unitOfWorkFactory, limitedItemsFunction, itemsCountFunction));
		}

		#region IDataLoader implementation

		public IList Items => ((IDataLoader)_threadDataLoader).Items;

		public PostLoadProcessing PostLoadProcessingFunc { set => ((IDataLoader)_threadDataLoader).PostLoadProcessingFunc = value; }
		public bool DynamicLoadingEnabled { get => ((IDataLoader)_threadDataLoader).DynamicLoadingEnabled; set => ((IDataLoader)_threadDataLoader).DynamicLoadingEnabled = value; }
		public int PageSize { get => ((IDataLoader)_threadDataLoader).PageSize; set => ((IDataLoader)_threadDataLoader).PageSize = value; }
		public int? ItemsCountForNextLoad { get => ((IDataLoader)_threadDataLoader).ItemsCountForNextLoad; set => ((IDataLoader)_threadDataLoader).ItemsCountForNextLoad = value; }

		public bool HasUnloadedItems => ((IDataLoader)_threadDataLoader).HasUnloadedItems;

		public bool FirstPage => ((IDataLoader)_threadDataLoader).FirstPage;

		public bool LoadInProgress => ((IDataLoader)_threadDataLoader).LoadInProgress;

		public bool TotalCountingInProgress => ((IDataLoader)_threadDataLoader).TotalCountingInProgress;

		public uint? TotalCount => ((IDataLoader)_threadDataLoader).TotalCount;

		public event EventHandler ItemsListUpdated {
			add {
				((IDataLoader)_threadDataLoader).ItemsListUpdated += value;
			}

			remove {
				((IDataLoader)_threadDataLoader).ItemsListUpdated -= value;
			}
		}

		public event EventHandler TotalCountChanged {
			add {
				((IDataLoader)_threadDataLoader).TotalCountChanged += value;
			}

			remove {
				((IDataLoader)_threadDataLoader).TotalCountChanged -= value;
			}
		}

		public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged {
			add {
				((IDataLoader)_threadDataLoader).LoadingStateChanged += value;
			}

			remove {
				((IDataLoader)_threadDataLoader).LoadingStateChanged -= value;
			}
		}

		public event EventHandler<LoadErrorEventArgs> LoadError {
			add {
				((IDataLoader)_threadDataLoader).LoadError += value;
			}

			remove {
				((IDataLoader)_threadDataLoader).LoadError -= value;
			}
		}

		public void CancelLoading() {
			((IDataLoader)_threadDataLoader).CancelLoading();
		}

		public IEnumerable<object> GetNodes(int entityId, IUnitOfWork uow) {
			return ((IDataLoader)_threadDataLoader).GetNodes(entityId, uow);
		}

		public void GetTotalCount() {
			((IDataLoader)_threadDataLoader).GetTotalCount();
		}

		public void LoadData(bool nextPage) {
			((IDataLoader)_threadDataLoader).LoadData(nextPage);
		}

		#endregion IDataLoader implementation


	}
}
