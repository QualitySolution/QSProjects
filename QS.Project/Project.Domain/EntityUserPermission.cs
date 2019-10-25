using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace QS.Project.Domain
{
	public class EntityUserPermission : EntityPermissionBase, IDomainObject 
	{
		public virtual int Id { get; set; }

		private UserBase user;
		[Display(Name = "Пользователь")]
		public virtual UserBase User {
			get => user;
			set => SetField(ref user, value, () => User);
		}
	}
}