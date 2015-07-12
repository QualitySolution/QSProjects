using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;

namespace QSOrmProject
{
	public class ChildUnitOfWork<TParentEntity, TChildEntity> : IChildUnitOfWorkGeneric<TParentEntity, TChildEntity> 
		where TChildEntity : IDomainObject, new()
		where TParentEntity : IDomainObject, new()
	{
		private List<object> ObjectToSave = new List<object> ();

		public object RootObject {
			get { return Root;}
		}

		public ParentReferenceGeneric<TParentEntity, TChildEntity> ParentReference { get; private set;}

		public IUnitOfWorkGeneric<TParentEntity> ParentUoW {
			get {
				return ParentReference.ParentUoWGeneric;
			}
		}

		public TChildEntity Root { get; private set;}

		public bool IsNew { get; private set;}

		public bool HasChanges
		{
			get
			{
				return IsNew || Session.IsDirty();
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

		public ChildUnitOfWork(ParentReferenceGeneric<TParentEntity, TChildEntity> parentReference )
		{
			IsNew = true;
			ParentReference = parentReference;
			Root = new TChildEntity();
		}

		public ChildUnitOfWork(ParentReferenceGeneric<TParentEntity, TChildEntity> parentReference, TChildEntity root)
		{
			IsNew = false;
			ParentReference = parentReference;
			Root = root;
		}

		public void Commit()
		{
			//TODO не реализовано
			foreach(var obj in ObjectToSave)
			{
				Session.SaveOrUpdate (obj);
			}
		}

		public void Dispose()
		{
			
		}

		public IQueryable<T> GetAll<T>() where T : IDomainObject
		{
			return Session.Query<T>();
		}

		public T GetById<T>(int id) where T : IDomainObject
		{
			return Session.Get<T>(id);
		}

		public void Save<TEntity>(TEntity entity) where TEntity : IDomainObject
		{
			ObjectToSave.Add (entity);

			if (RootObject.Equals(entity))
			{
				Commit();
				OrmMain.DelayedNotifyObjectUpdated (ParentUoW.RootObject, entity);
			}
			else
			{
				OrmMain.DelayedNotifyObjectUpdated(RootObject, entity);
			}
		}

		public void Save()
		{
			Save(Root);
		}

		public void Delete<T>(int id) where T : IDomainObject
		{
			Session.Delete(Session.Load<T>(id));
		}
	}
}

