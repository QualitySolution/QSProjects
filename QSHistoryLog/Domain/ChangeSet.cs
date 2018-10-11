using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.Project.Domain;

namespace QS.HistoryLog.Domain
{
	public class ChangeSet : IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		public virtual UserBase User { get; set; }

		public virtual string UserLogin { get; set; }

		public virtual string ActionName { get; set; }

		public virtual IList<ChangedEntity> Entities { get; set; } = new List<ChangedEntity>();

		public virtual string UserName { get{
				return User?.Name ?? UserLogin;
			}}

		public ChangeSet ()
		{
		}

		public ChangeSet(string actionName, UserBase user = null, string login = null)
		{
			User = user;
			UserLogin = login ?? user?.Login;
			ActionName = actionName;
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

