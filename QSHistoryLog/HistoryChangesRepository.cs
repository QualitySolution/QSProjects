using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QSHistoryLog.Domain;
using QSOrmProject;
using QSOrmProject.Domain;

namespace QSHistoryLog
{
	public static class HistoryChangesRepository
	{
		/// <summary>
		/// Возвращает набор изменений объектов указанных id
		/// </summary>
		public static IList<HistoryChangeSet> GetHistoryChanges<T>(IUnitOfWork uow, int[] idList)
			where T : IDomainObject
		{
			var changes = uow.Session.QueryOver<HistoryChangeSet>()
			                 .Where(x => x.ObjectName == typeof(T).Name)
			                 .WhereRestrictionOn(x => x.ItemId).IsIn(idList)
			                 .List().ToList();
			var users = uow.Session.QueryOver<User>().WhereRestrictionOn(x => x.Id).IsIn(changes.Select(y => y.UserId).ToList()).List();
			changes.ForEach((obj) => {
				obj.UserName = users.Where(x => x.Id == obj.UserId).Select(x => x.Name).FirstOrDefault();
			});
			return changes;
		}
	}
}
