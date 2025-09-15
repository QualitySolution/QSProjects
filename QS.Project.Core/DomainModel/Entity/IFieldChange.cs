using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QS.DomainModel.Entity {
	public interface IFieldChange {
		public string Path { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public int? OldId { get; set; }
		public int? NewId { get; set; }
		public FieldChangeType Type { get; set; }
		public IChangedEntity Entity { get; set; }
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
