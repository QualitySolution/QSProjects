using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QSOrmProject.DomainModel
{
	public class ChildUnitOfWork<TParentEntity, TChildEntity> : IChildUnitOfWorkGeneric<TParentEntity, TChildEntity> 
		where TChildEntity : class, IDomainObject, new()
		where TParentEntity : IDomainObject, new()
	{
		private List<object> ObjectToSave = new List<object> ();
		private List<object> ObjectToDelete = new List<object> ();
		private TChildEntity externalRootVersion;

		public object RootObject {
			get { return Root; }
		}

		public bool IsAlive { get{ return ParentUoW.IsAlive;
			}}

		public ParentReferenceGeneric<TParentEntity, TChildEntity> ParentReference { get; private set; }

		public IUnitOfWorkGeneric<TParentEntity> ParentUoW {
			get {
				return ParentReference.ParentUoWGeneric;
			}
		}

		public TChildEntity Root { get; private set; }

		public bool IsNew { get; private set; }

		public bool HasChanges {
			get {
				return IsNew || !ObjectCloner.CompareFields (Root, externalRootVersion); 
			}
		}

		// TODO Ксожалению не работает при открытии второй сессии возвращается первая закрытая.
		// Проблема описана здесь http://osdir.com/ml/nhibernate-development/2010-02/msg00131.html
		// session = ParentUoW.Session.GetSession (EntityMode.Poco);
		// класс переделан на ручное использование родительской сессии.

		public ISession Session {
			get {
				return ParentUoW.Session;
			}
		}

		public UnitOfWorkTitle ActionTitle { get; protected set; }

		public bool CanCheckIfDirty { get; set; }

		public ChildUnitOfWork(ParentReferenceGeneric<TParentEntity, TChildEntity> parentReference, UnitOfWorkTitle title)
		{
			IsNew = true;
			ActionTitle = title;
			ParentReference = parentReference;
			Root = new TChildEntity ();
		}

		public ChildUnitOfWork (ParentReferenceGeneric<TParentEntity, TChildEntity> parentReference, TChildEntity root, UnitOfWorkTitle title)
		{
			IsNew = false;
			ActionTitle = title;
			ParentReference = parentReference;
			externalRootVersion = root;
			Root = ObjectCloner.Clone (externalRootVersion);
		}

		public void Commit ()
		{
			if (IsNew) {
				if(ParentReference.AddNewChild == null)
					throw new InvalidOperationException(String.Format("Для {0} не настроена функция AddNewChild. Без нее, подчиненный Uow, не может работать.", ParentReference.GetType()));
					
				ParentReference.AddNewChild (ParentUoW.Root, Root);
				IsNew = false;
			}

			foreach (var obj in ObjectToDelete) {
				Session.Delete (obj);
			}

			foreach (var obj in ObjectToSave) {
				Session.SaveOrUpdate (obj);
			}
		}

		public void Dispose ()
		{
			
		}

		public IQueryable<T> GetAll<T> () where T : IDomainObject
		{
			return Session.Query<T> ();
		}

		public IQueryOver<T, T> Query<T>() where T : class
		{
			return Session.QueryOver<T>();
		}

		public IQueryOver<T, T> Query<T>(Expression<Func<T>> alias) where T : class
		{
			return Session.QueryOver<T>(alias);
		}

		public T GetById<T> (int id) where T : IDomainObject
		{
			return Session.Get<T> (id);
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

		public object GetById (Type clazz, int id)
		{
			return Session.Get (clazz, id);
		}

		public void Save<TEntity> (TEntity entity, bool orUpdate = true) where TEntity : IDomainObject
		{
			if(!orUpdate)
				throw new NotImplementedException("orUpdate = false не реализовано для ChildUnitOfWork");

			if (RootObject.Equals (entity)) {
				if (externalRootVersion == null)
					externalRootVersion = Root;
				else
					ObjectCloner.FieldsCopy (Root, ref externalRootVersion);
				ObjectToSave.Add (externalRootVersion);
				Commit ();
				OrmMain.DelayedNotifyObjectUpdated (ParentUoW.RootObject, entity);
			} else {
				ObjectToSave.Add (entity);
				OrmMain.DelayedNotifyObjectUpdated (RootObject, entity);
			}
		}

		public void TrySave(object entity, bool orUpdate = true)
		{
			throw new NotImplementedException ();
		}

		public void TryDelete(object entity)
		{
			throw new NotImplementedException ();
		}

		public void Save ()
		{
			Save (Root);
		}

		public void Delete<T> (int id) where T : IDomainObject
		{
			Delete (Session.Load<T> (id));
		}

		public void Delete<TEntity> (TEntity entity) where TEntity : IDomainObject
		{
			ObjectToDelete.Add (entity);
		}
	}
}

