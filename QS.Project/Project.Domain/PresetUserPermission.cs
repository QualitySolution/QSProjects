using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Permissions;

namespace QS.Project.Domain
{
	public class PresetUserPermission : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private UserBase user;
		[Display(Name = "Пользователь")]
		public virtual UserBase User {
			get => user;
			set => SetField(ref user, value, () => User);
		}

		private string permissionName;
		[Display(Name = "Право")]
		public virtual string PermissionName {
			get => permissionName;
			set => SetField(ref permissionName, value, () => PermissionName);
		}


		#region Вычисляемые

		public PresetUserPermissionSource PermissionSource {
			get {
				if(!PermissionsSettings.PresetPermissions.ContainsKey(PermissionName)) {
					return null;
				}
				return PermissionsSettings.PresetPermissions[PermissionName];
			}
		}

		public string DisplayName => PermissionSource != null ? PermissionSource.DisplayName : PermissionName;

		public bool IsLostPermission => PermissionSource == null;

		#endregion

	}
}
