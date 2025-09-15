using QS.DomainModel.Entity;
using QS.SqlLog.Domain;
using System.Threading.Tasks;

namespace QS.SqlLog.Interfaces {
	public interface IChangeSetPersister {
		void Save(IChangeSet changeSet);
		Task SaveAsync(IChangeSet changeSet);
	}
}
