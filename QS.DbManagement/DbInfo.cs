namespace QS.DbManagement {
	public class DbInfo {
		public string Title { get; set; }
		public string BaseName { get; set; }
		public int BaseId { get; set; }
		public string Version { get; set; }

		/// <summary>
		/// Может ли пользователь создавать новые базы на сервере, откуда получен этот DbInfo.
		/// </summary>
		public bool CanCreateDatabase { get; set; }
	}
}
