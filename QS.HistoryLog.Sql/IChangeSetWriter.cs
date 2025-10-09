using System.Threading.Tasks;
using QS.HistoryLog.Domain;

namespace QS.HistoryLog {
	public interface IChangeSetWriter {
		void Save(IChangeSetToSave changeSet);
		Task SaveAsync(IChangeSetToSave changeSet);
	}
}
