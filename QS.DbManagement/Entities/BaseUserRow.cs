namespace QS.DbManagement.Entities {
	internal class BaseUserRow {
		public int Id { get; set; }
		public string Name { get; set; }
		public string Login { get; set; }
		public bool Deactivated { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
		public bool Admin { get; set; }
	}
}
