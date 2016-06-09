using System;
using System.Collections.Generic;

namespace QSOrmProject
{
	public interface IUnitOfWork : IDisposable 
	{
		NHibernate.ISession Session{ get;}

		object RootObject{ get;}

		bool IsNew { get;}

		bool IsAlive { get;}

		bool HasChanges{ get;}

		void Save<TEntity>(TEntity entity) where TEntity : IDomainObject;
		void Save();

		/// <summary>
		/// Пытаемся сохранить сущность в виде объекта, без указания типа сущности.
		/// По возможности используйте дженерик метод Save().
		/// </summary>
		void TrySave(object entity);

		/// <summary>
		/// Пытаемся удалить сущность в виде объекта, неизвестного типа.
		/// По возможности используйте дженерик метод Delete().
		/// </summary>
		void TryDelete(object entity);

		T GetById<T>(int id) where T : IDomainObject;

		IList<T> GetById<T>(int[] ids) where T : class, IDomainObject;

		IList<T> GetById<T>(IEnumerable<int> ids) where T : class, IDomainObject;

		object GetById(Type clazz, int id);

		void Commit();

		void Delete<TEntity>(TEntity entity) where TEntity : IDomainObject;
	}
}

