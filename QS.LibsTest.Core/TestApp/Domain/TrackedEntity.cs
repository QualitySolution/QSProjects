using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace QS.Test.TestApp.Domain {
	[HistoryTrace]
	public class TrackedEntity : IDomainObject {
			public virtual int Id { get; set; }
			[Display(Name = "Название")]
			public virtual string Name { get; set; }
		
			public virtual string Description { get; set; }
			public virtual SimpleEntity AnotherEntity { get; set; }
	}
}
