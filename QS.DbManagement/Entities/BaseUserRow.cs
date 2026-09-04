namespace QS.DbManagement.Entities {
	internal class BaseUserRow {
		public string Login { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public bool Admin { get; set; }
		public bool Deactivated { get; set; }
	}
}
