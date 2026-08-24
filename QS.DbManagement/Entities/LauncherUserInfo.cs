namespace QS.DbManagement.Entities {
	internal class LauncherUserInfo {
		public int Id { get; set; }
		public string Login { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }

		public bool IsAdmin { get; set; }
		public bool Disabled { get; set; }
	}
}
