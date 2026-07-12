namespace QS.DbManagement.Entities {
	public class DbUserBaseAccess {
		/// <summary>Идентификатор базы в облаке</summary>
		public int BaseId { get; set; }

		/// <summary>Имя базы на сервере</summary>
		public string BaseName { get; set; }

		public string Title { get; set; }

		public bool HasAccess { get; set; }

		public bool IsAdmin { get; set; }

		public bool ReadOnly { get; set; }
		public bool CanEdit { get; set; } = true;
		public string Name { get; set; }
		public string Email { get; set; }

		public bool CanDelete { get; set; } = true;
		public bool CanAccountingSettings { get; set; } = true;
		public bool CanChangeDocumentDate { get; set; } = true;
	}
}
