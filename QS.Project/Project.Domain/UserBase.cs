using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Repositories;

namespace QS.Project.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "пользователи",
	Nominative = "пользователь")]
	public class UserBase : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Имя пользователя")]
		[Required(ErrorMessage = "Имя пользователя должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		string login;

		[Display(Name = "Логин")]
		[Required(ErrorMessage = "Логин пользователя должен быть заполнен.")]
		public virtual string Login {
			get { return login; }
			set { SetField(ref login, value, () => Login); }
		}

		bool deactivated;

		[Display(Name = "Отключен")]
		public virtual bool Deactivated {
			get { return deactivated; }
			set { SetField(ref deactivated, value, () => Deactivated); }
		}

		bool isAdmin;

		[Display(Name = "Администратор")]
		public virtual bool IsAdmin {
			get { return isAdmin; }
			set { SetField(ref isAdmin, value, () => IsAdmin); }
		}

		private string email;
		[Display(Name = "Электронная почта")]
		public virtual string Email {
			get => email;
			set => SetField(ref email, value, () => Email);
		}

		private string description;

		[Display(Name = "Описание")]
		public virtual string Description {
			get => description;
			set => SetField(ref description, value, () => Description);
		}


		private ReadOnlyDictionary<string, bool> permissions;
		public virtual IReadOnlyDictionary<string, bool> Permissions => permissions;

		#endregion

		public UserBase()
		{
			permissions = new ReadOnlyDictionary<string, bool>(new Dictionary<string, bool>());
		}

		public virtual void LoadUserPermissions()
		{
			if (Id == 0) {
				return;
			}
			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var userPresetPermission = UserPermissionRepository.GetUserAllPresetPermissions(uow, Id)
					.Where(x => !x.IsLostPermission)
					.ToDictionary(x => x.PermissionName);
				var userPermissions = new Dictionary<string, bool>();
				foreach (var item in PermissionsSettings.PresetPermissions.Keys) {
					userPermissions.Add(item, userPresetPermission.ContainsKey(item));
				}
				permissions = new ReadOnlyDictionary<string, bool>(userPermissions);
			}
		}
	}

	public class UserBaseEqualityComparer : IEqualityComparer<UserBase>
	{
		public bool Equals(UserBase x, UserBase y) => x.Id == y.Id;
		public int GetHashCode(UserBase obj) => obj.Id.GetHashCode();
	}
}