using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.Project.Domain;

namespace QS.HistoryLog.Domain
{
	public class ChangeSet : ChangeSetBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Поле используется только для сохранения, так как в этот момент не нужен полный класс достаточно id.
		/// </summary>
		public virtual int UserId{ get; set; }
		public virtual UserBase User { get; set; }

		public virtual IList<ChangedEntity> Entities { get; set; } = new List<ChangedEntity>();

		public virtual string UserName { get{
				return User?.Name ?? UserLogin;
			}}

		public ChangeSet ()
		{
		}

		public ChangeSet(string actionName, int userId, string login)
		{
			UserId = userId;
			UserLogin = login;
			ActionName = actionName;

			//При сохранении в базу обрезаем длину действия до размера колонки.
			if(ActionName != null && ActionName.Length > 100)
				ActionName = ActionName.Substring(0, 97) + "...";
		}

		public virtual void AddChangeEntities(IEnumerable<ChangedEntity> changes)
		{
			foreach(var entity in changes)
			{
				entity.ChangeSet = this;
				Entities.Add(entity);
			}
		}
	}
}

