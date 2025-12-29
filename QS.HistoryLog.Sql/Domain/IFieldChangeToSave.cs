using System.ComponentModel.DataAnnotations;

namespace QS.HistoryLog.Domain {
	public interface IFieldChangeToSave {
		string Path { get; }
		string OldValue { get; }
		string NewValue { get; }
		int? OldId { get; }
		int? NewId { get; }
		FieldChangeType Type { get; }
	}

	public enum FieldChangeType {
		[Display(Name = "Добавлено")]
		Added,
		[Display(Name = "Изменено")]
		Changed,
		[Display(Name = "Очищено")]
		Removed,
		[Display(Name = "Без изменений")]
		Unchanged
	}
}
