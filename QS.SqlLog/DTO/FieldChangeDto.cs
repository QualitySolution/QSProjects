namespace QS.SqlLog.Domain {
	public class FieldChangeDto  {
		public string Path { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public int? OldId { get; set; }
		public int? NewId { get; set; }
		public string Type { get; set; }
	}
}
