namespace QS.DbManagement.Entities {
	public class DbUserBaseAccess {
		public int BaseId { get; set; }

		public string BaseName { get; set; }

		public string Title { get; set; }

		public bool HasAccess { get; set; }

		public bool IsAdmin { get; set; }

		public bool ReadOnly { get; set; }
	}
}
