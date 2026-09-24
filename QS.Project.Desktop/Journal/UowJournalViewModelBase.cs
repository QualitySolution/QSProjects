using System;
using System.Diagnostics;
using System.Linq;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;

namespace QS.Journal {
	public class UowJournalViewModelBase<TNode> : JournalViewModelBase<TNode>, IDisposable
		where TNode : class
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public UowJournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IEntityChangeWatcher changeWatcher) : base(navigation) {
			ChangeWatcher = changeWatcher;
			UnitOfWorkFactory = unitOfWorkFactory;
		}

		#region Uow
		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;

		private IUnitOfWork unitOfWork;

		public virtual IUnitOfWork UoW { 
			get {
				if(unitOfWork == null) {
					var timer = Stopwatch.StartNew();
					unitOfWork = UnitOfWorkFactory.Create(Title);
					logger.Debug($"Журнал {GetType().Name}: UnitOfWork создан за {timer.Elapsed.TotalMilliseconds:F0} мс.");
				}

				return unitOfWork;
			}
			set => unitOfWork = value;
		}
		#endregion
		
		#region SubscribeOnEntity
		protected readonly IEntityChangeWatcher ChangeWatcher;
		
		public void UpdateOnChanges(params Type[] entityTypes)
		{
			ChangeWatcher?.UnsubscribeAll(this);
			ChangeWatcher?.BatchSubscribeOnEntity(OnEntitiesUpdated, entityTypes);
		}
		
		private void OnEntitiesUpdated(EntityChangeEvent[] changeEvents)
		{
			var changesDelta = changeEvents.Any(x => x.DeleteEvent == null)
				? changeEvents.Any(x => x.InsertEvent == null) ? 0 : 1
				: -1;

			DataLoader.ItemsCountForNextLoad = DataLoader.Items.Count + changesDelta;

			var dataLoaderType = DataLoader.GetType();
			var isThreadDataLoader = dataLoaderType.IsGenericType && dataLoaderType.GetGenericTypeDefinition() == typeof(ThreadDataLoader<>);

			var needResetItemsCountForNextLoad = isThreadDataLoader	&& !DataLoader.FirstPage && DataLoader.PageSize >= DataLoader.ItemsCountForNextLoad;

			Refresh(needResetItemsCountForNextLoad);
		}
		#endregion
		
		public virtual void Dispose() {
        	ChangeWatcher?.UnsubscribeAll(this);
	        UoW?.Dispose();
        }
	}
}
