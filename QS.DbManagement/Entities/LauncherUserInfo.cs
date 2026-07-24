using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DbManagement.Entities {
	internal class LauncherUserInfo {
		public int Id { get; set; }
		public string Login { get; set; }
		public int AccountId { get; set; }
		public string AccountName { get; set; }
		public string PasswordHash { get; set; }
		public bool IsAccountAdmin { get; set; }
	}
}
