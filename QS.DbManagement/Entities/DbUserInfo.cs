using System;

namespace QS.DbManagement.Entities {
	public class DbUserInfo {
		public string Login { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Post { get; set; }
		public string Comment { get; set; }

		/// <summary>не может входить</summary>
		public bool Disabled { get; set; }

		/// <summary>может управлять другими пользователями</summary>
		public bool IsAdmin { get; set; }
		/// <summary>текущий пользователь подключения</summary>
		public bool IsCurrentUser { get; set; }

		/// <summary>затронутые поля изменениями с вьюхи</summary>
		public DbUserFields DirtyFields { get; set; } = DbUserFields.None;
	}

	[Flags]
	public enum DbUserFields {
		None = 0,
		Name = 1,
		Email = 2,
		Phone = 4,
		Post = 8,
		Comment = 16,
		AdminFlag = 32,
		Disabling = 64,
		BaseReadOnly = 128
	}
}
