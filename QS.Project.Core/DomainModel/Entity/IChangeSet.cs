using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DomainModel.Entity {
	public interface IChangeSet {
		public string ActionName { get; set; }
		public int UserId { get; set; }
		public string UserLogin { get; set; }
		public ICovariantCollection<IChangedEntity> Entities { get; } 
	}
}
