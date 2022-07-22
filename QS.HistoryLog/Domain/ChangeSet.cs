using System.Collections.Generic;
using QS.Project.Domain;

namespace QS.HistoryLog.Domain
{
	public class ChangeSet : ChangeSetBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual IList<ChangedEntity> Entities { get; set; } = new List<ChangedEntity>();

		public virtual UserBase User { get; set; }

		public virtual string UserName {
			get {
				return User?.Name ?? UserLogin;
			}
		}

		public ChangeSet () { }

		public ChangeSet(string actionName, UserBase user = null, string login = null)
		{
			User = user;
			UserLogin = login ?? user?.Login;
			ActionName = actionName;

			//При сохранении в базу обрезаем длину действия до размера колонки.
			if(ActionName != null && ActionName.Length > 100) {
				ActionName = ActionName.Substring(0, 97) + "...";
			}
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

