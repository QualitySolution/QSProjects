using QS.DomainModel.Entity;

namespace QS.SqlLog.Domain {
	public class FieldChangeDto : IFieldChange {
		public string Path { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public int? OldId { get; set; }
		public int? NewId { get; set; }
		public FieldChangeType Type { get; set; }
		public IChangedEntity Entity { get; set; } = null;
	}
}
