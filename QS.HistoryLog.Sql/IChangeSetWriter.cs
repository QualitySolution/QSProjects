using System.Threading.Tasks;
using QS.DomainModel.Entity;

namespace QS.HistoryLog {
	public interface IChangeSetWriter {
		void Save(IChangeSet changeSet);
		Task SaveAsync(IChangeSet changeSet);
	}
}
