using QS.SqlLog.Domain;
using System.Threading.Tasks;

namespace QS.SqlLog.Interfaces {
	public interface IChangeSetPersister {
		void Save(ChangeSetDto changeSet);
		Task SaveAsync(ChangeSetDto changeSet);
	}
}
