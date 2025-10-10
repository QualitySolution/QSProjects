
namespace QS.HistoryLog.Domain {
	public class SimpleFieldChange : IFieldChangeToSave {
		public string Path { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public int? OldId { get; set; }
		public int? NewId { get; set; }
		public FieldChangeType Type { get; set; }
		public IChangedEntityToSave Entity { get; set; }
	}
}
