namespace QS.DbManagement.Entities {
	internal struct BaseRow {
		public int AccountId;
		public byte ProductId;
		public string Title;
		public string Name;
		public string Version;

		public BaseRow(int accountId, byte productId, string title, string name, string version) {
			AccountId = accountId;
			ProductId = productId;
			Title = title;
			Name = name;
			Version = version;
		}
	}
}
