using System.Collections.Generic;
using QS.Project.Domain;

namespace QS.HistoryLog.Domain
{
	public class ChangeSet : ChangeSetBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual IList<ChangedEntity> Entities { get; set; } = new List<ChangedEntity>();

		public ChangeSet () { }

		public ChangeSet(string actionName, UserBase user = null, string login = null)
		{
			SetActionAndUserProperties(actionName, user, login);
		}

		public virtual void AddChange(params ChangedEntity[] changes)
		{
			foreach(var entity in changes)
			{
				entity.ChangeSet = this;
				Entities.Add(entity);
			}
		}
	}
}

