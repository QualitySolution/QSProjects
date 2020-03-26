using System;
using System.Linq;
using NHibernate;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;

namespace QS.DomainModel.UoW
{
	/// <summary>
	/// Код решения частично взять https://nhibernate.info/doc/howto/various/finding-dirty-properties-in-nhibernate.html
	/// и https://stackoverflow.com/a/35697565
	/// </summary>
	public static class NhibernateExtend
	{
		/// <summary>
		/// Check whether a session is "dirty" without triggering a flush
		/// </summary>
		/// <param name="session"></param>
		/// <returns>true if the session is "dirty", meaning that it will update the database when flushed</returns>
		/// <remarks>
		/// The rationale behind this is the need to determine if there's anything to flush to the database without actually
		/// running through the Flush process.  The problem with a premature Flush is that one may want to collect changes
		/// to persistent objects and only start a transaction later on to flush them.  I have this in a Winforms application
		/// and this method allows me to notify the user whether he has made changes that need saving while not leaving a
		/// transaction open while he works, which can cause locking issues.
		/// <para>
		/// Note that the check for dirty collections may give false positives, which is good enough for my purposes but 
		/// coule be improved upon using calls to GetOrphans and other persistent-collection methods.</para>
		/// </remarks>
		public static bool IsDirtyNoFlush(this ISession session)
		{
			var pc = session.GetSessionImplementation().PersistenceContext;
			if(pc.EntitiesByKey.Values.ToList().Any(o => IsDirtyEntity(session, o)))
				return true;

			return pc.CollectionEntries.Keys.Cast<IPersistentCollection>()
				.Any(coll => coll.WasInitialized && coll.IsDirty);
		}

		public static Boolean IsDirtyEntity(this ISession session, Object entity)
		{
			ISessionImplementor sessionImpl = session.GetSessionImplementation();
			IPersistenceContext persistenceContext = sessionImpl.PersistenceContext;
			EntityEntry oldEntry = persistenceContext.GetEntry(entity);
			string className = oldEntry.EntityName;
			IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);

			if(oldEntry == null && entity is INHibernateProxy) {
				INHibernateProxy proxy = entity as INHibernateProxy;
				Object obj = sessionImpl.PersistenceContext.Unproxy(proxy);
				oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);
			}

			object[] oldState = oldEntry.LoadedState;
			if(oldState == null) {
				return false;
			}

			object[] currentState = persister.GetPropertyValues(entity);
			int[] dirtyProps = oldState.Select((o, i) => Equals(oldState[i], currentState[i]) ? -1 : i).Where(x => x >= 0).ToArray();

			return (dirtyProps != null && dirtyProps.Length > 0);
		}
	}
}
