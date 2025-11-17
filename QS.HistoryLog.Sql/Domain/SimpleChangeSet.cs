using System.Collections.Generic;

namespace QS.HistoryLog.Domain
{
	/// <summary>
	/// Простой класс для сохранения истории изменений без привязки к ORM.
	/// </summary>
	public class SimpleChangeSet : IChangeSetToSave {
		public string ActionName { get; set; }
		public int? UserId { get; set; }
		public string UserLogin { get; set; }
		public List<SimpleChangedEntity> Entities { get; } = new List<SimpleChangedEntity>();
		IEnumerable<IChangedEntityToSave> IChangeSetToSave.Entities => Entities;
	}
}

