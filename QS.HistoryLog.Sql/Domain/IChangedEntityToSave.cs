using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.HistoryLog.Domain {
	public interface IChangedEntityToSave {
		DateTime ChangeTime { get; }
		EntityChangeOperation Operation { get; }
		string EntityClassName { get; }
		int EntityId { get; }
		string EntityTitle { get; }
		IEnumerable<IFieldChangeToSave> Changes { get; }
	}

	public enum EntityChangeOperation {
		[Display(Name = "Создание")]
		Create,
		[Display(Name = "Изменение")]
		Change,
		[Display(Name = "Удаление")]
		Delete
	}
}
