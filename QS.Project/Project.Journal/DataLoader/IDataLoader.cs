using QS.DomainModel.UoW;
using System;
using System.Collections;
using System.Collections.Generic;

namespace QS.Project.Journal.DataLoader
{
	public interface IDataLoader
	{
		IList Items { get; }

		/// <summary>
		/// Коллекция с данными была обновлена. Внимание, событие может приходить из другого потока.
		/// </summary>
		event EventHandler ItemsListUpdated;

		IEnumerable<object> GetNodes(int entityId, IUnitOfWork uow);
	}
}
