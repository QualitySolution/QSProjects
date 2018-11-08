using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QS.DomainModel.Entity;

namespace QS.DomainModel.UoW
{
	public interface IUnitOfWork : IDisposable 
	{
		UnitOfWorkTitle ActionTitle { get; }

		NHibernate.ISession Session{ get;}

		object RootObject{ get;}

		bool IsNew { get;}

		bool IsAlive { get;}

		/// <summary>
		/// Свойство для обхода фичи-бага nHibernate, при котором при вызове
		/// метода IsDirty() происходила запись объектов в базу.
		/// </summary>
		/// <value><c>true</c> if can check if dirty; otherwise, <c>false</c>.</value>
		bool CanCheckIfDirty { get; set; }

		bool HasChanges { get;}

		/// <param name="orUpdate">
		/// По умолчанию установлен в true это значит то будет вызываться метод SaveOrUpdate вместо Save.
		/// Этот параметр нуже тогда когда мы сохраняем много новых объектов, при использовании метода SaveOrUpdate Nhibernate перед INSERT 
		/// делает SELECT что бы проверить нет ли уже объекта для обновления. Что при большом количестве объектов приводит к задержкам сохранения.
		/// </param>
		void Save<TEntity>(TEntity entity, bool orUpdate = true) where TEntity : IDomainObject;
		void Save();

		/// <summary>
		/// Пытаемся сохранить сущность в виде объекта, без указания типа сущности.
		/// По возможности используйте дженерик метод Save().
		/// </summary>
		void TrySave(object entity, bool orUpdate = true);

		/// <summary>
		/// Пытаемся удалить сущность в виде объекта, неизвестного типа.
		/// По возможности используйте дженерик метод Delete().
		/// </summary>
		void TryDelete(object entity);

		IQueryable<T> GetAll<T> () where T : IDomainObject;

		NHibernate.IQueryOver<T, T> Query<T>() where T : class;
		NHibernate.IQueryOver<T, T> Query<T>(Expression<Func<T>> alias) where T : class;

		T GetById<T>(int id) where T : IDomainObject;

		IList<T> GetById<T>(int[] ids) where T : class, IDomainObject;

		IList<T> GetById<T>(IEnumerable<int> ids) where T : class, IDomainObject;

		object GetById(Type clazz, int id);

		void Commit();

		void Delete<TEntity>(TEntity entity) where TEntity : IDomainObject;
	}
}