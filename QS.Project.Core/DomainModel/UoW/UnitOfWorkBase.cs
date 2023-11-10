using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.Project.DB;

[assembly:InternalsVisibleTo("QS.LibsTest.Core")]
namespace QS.DomainModel.UoW
{
	public abstract class UnitOfWorkBase : IUnitOfWorkTracked
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		static UnitOfWorkBase()
		{
			BusinessObjectPreparer.Init();
		}

		protected UnitOfWorkBase(ISessionProvider sessionProvider, IMainThreadDispatcher mainThreadDispatcher)
		{
			if(mainThreadDispatcher is null) throw new ArgumentNullException(nameof(mainThreadDispatcher));
			this.SessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
			EventsTracker = new SingleUowEventsTracker(mainThreadDispatcher);
		}

		public event EventHandler<EntityUpdatedEventArgs> SessionScopeEntitySaved;

		protected virtual ITransaction transaction { get; set; }

		protected ISession session;

		protected ISessionProvider SessionProvider;

		public SingleUowEventsTracker EventsTracker { get; } 

		public bool IsNew { get; protected set; }

		public UnitOfWorkTitle ActionTitle { get; protected set; }

		public bool IsAlive {
			get {
				return Session.IsOpen;
			}
		}

		public virtual ISession Session {
			get {
				if(session == null) {
					session = SessionProvider.OpenSession();
					UowWatcher.RegisterUow(this);
				}

				return session;
			}
		}

		public virtual void Commit()
		{
			if(transaction == null || !transaction.IsActive) {
				logger.Warn("Попытка комита закрытой транзацкии.");
				return;
			}

			try {
				transaction.Commit();

				IsNew = false;
				GlobalUowEventsTracker.OnPostCommit(this);
			} 
			catch(Exception ex)
			{
				logger.Error(ex, $"Исключение при комите в {GetType()}:{ActionTitle?.UserActionTitle}(Created:{ActionTitle?.CallerMemberName}:{ActionTitle?.CallerLineNumber})");
				if(transaction.IsActive)
					transaction.Rollback();
				throw;
			} 
			finally {
				transaction.Dispose();
				transaction = null;
			}
		}

		public async Task CommitAsync() {
			if(transaction == null || !transaction.IsActive) {
				logger.Warn("Попытка комита закрытой транзацкии.");
				return;
			}

			try {
				await transaction.CommitAsync();

				IsNew = false;
				GlobalUowEventsTracker.OnPostCommit(this);
			}
			catch(Exception ex) {
				logger.Error(ex, $"Исключение при комите в {GetType()}:{ActionTitle?.UserActionTitle}(Created:{ActionTitle?.CallerMemberName}:{ActionTitle?.CallerLineNumber})");
				if(transaction.IsActive)
					await transaction.RollbackAsync();
				throw;
			}
			finally {
				transaction.Dispose();
				transaction = null;
			}
		}

		protected virtual void DisposeUoW()
		{
			if(transaction != null) {
				if(!transaction.WasCommitted && !transaction.WasRolledBack
				&& transaction.IsActive && session.Connection.State == System.Data.ConnectionState.Open)
					transaction.Rollback();

				transaction.Dispose();
				transaction = null;
			}
			Session.Dispose();
			UowWatcher.UnregisterUow(this);
		}

		public void Dispose()
		{
			DisposeUoW();
		}

		public IQueryable<T> GetAll<T>() where T : IDomainObject
		{
			return Session.Query<T>();
		}

		public IQueryOver<T, T> Query<T>() where T : class
		{
			return Session.QueryOver<T>();
		}

		public IQueryOver<T, T> Query<T>(Expression<Func<T>> alias) where T : class
		{
			return Session.QueryOver<T>(alias);
		}

		public T GetById<T>(int id) where T : IDomainObject
		{
			return Session.Get<T>(id);
		}

		public T GetInSession<T>(T origin) where T : class, IDomainObject {
			if(origin == null)
				return null;
			return Session.Get<T>(origin.Id);
		}

		public IList<T> GetById<T>(IEnumerable<int> ids) where T : class, IDomainObject
		{
			return GetById<T>(ids.ToArray());
		}

		public IList<T> GetById<T>(int[] ids) where T : class, IDomainObject
		{
			return Session.QueryOver<T>()
				.Where(x => x.Id.IsIn(ids)).List();
		}

		public object GetById(Type clazz, int id)
		{
			return Session.Get(clazz, id);
		}

		public virtual void Save(object entity, bool orUpdate = true)
		{
			OpenTransaction();

			if(orUpdate)
				Session.SaveOrUpdate(entity);
			else
				Session.Save(entity);

			RaiseSessionScopeEntitySaved(new object[] { entity });
		}

		public virtual async Task SaveAsync(object entity, bool orUpdate = true) {
			OpenTransaction();

			if(orUpdate)
				await Session.SaveOrUpdateAsync(entity);
			else
				await Session.SaveAsync(entity);

			RaiseSessionScopeEntitySaved(new object[] { entity });
		}

		public void Delete<T>(int id) where T : IDomainObject
		{
			Delete(Session.Load<T>(id));
		}

		public void Delete(object entity)
		{
			OpenTransaction();
			Session.Delete(entity);
		}

		public async Task DeleteAsync(object entity) {
			OpenTransaction();
			await Session.DeleteAsync(entity);
		}

		internal virtual void OpenTransaction()
		{
			if(transaction == null || !transaction.IsActive) {
				transaction = Session.BeginTransaction();
			}
		}

		public void RaiseSessionScopeEntitySaved(object[] entity)
		{
			SessionScopeEntitySaved?.Invoke(this, new EntityUpdatedEventArgs(entity));
		}
	}
}

