using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QS.DomainModel.Entity {
	public interface IChangedEntity {
		public DateTime ChangeTime { get; set; }
		public EntityChangeOperation Operation { get; set; }
		public string EntityClassName { get; set; }
		public int EntityId { get; set; }
		public string EntityTitle { get; set; }
		public ICovariantCollection<IFieldChange> Changes { get; set; }
		public IChangeSet ChangeSet { get; set; }
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
