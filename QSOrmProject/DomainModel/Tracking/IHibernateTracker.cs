using System;
using NHibernate.Event;
using QSOrmProject;

namespace QS.DomainModel.Tracking
{
	public interface IHibernateTracker
	{
		void OnPostInsert(PostInsertEvent insertEvent);
		void OnPostUpdate(PostUpdateEvent updateEvent);
		void OnPostDelete(PostDeleteEvent deleteEvent);
		void Reset();

		/// <summary>
		/// Сохраняем журнал изменений через отдельный UnitOfWork.
		/// </summary>
		/// <param name="userUoW">Необходим только для получения названия пользовательских действий.</param>
		void SaveChangeSet(IUnitOfWork userUoW);
	}
}
