using QS.Project.Domain;
using QS.DomainModel.Entity;

namespace QS.HistoryLog.Domain
{
	public class ChangeSetBase : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual UserBase User { get; set; }

		public virtual string UserLogin { get; set; }

		public virtual string ActionName { get; set; }

		public virtual string UserName {
			get {
				return User?.Name ?? UserLogin;
			}
		}

		protected virtual void SetActionAndUserProperties(string actionName, UserBase user, string login) {
			User = user;
			UserLogin = login ?? user?.Login;
			ActionName = actionName;

			//При сохранении в базу обрезаем длину действия до размера колонки.
			if(ActionName != null && ActionName.Length > 100) {
				ActionName = ActionName.Substring(0, 97) + "...";
			}
		}
	}
}
