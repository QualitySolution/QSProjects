using System.Runtime.CompilerServices;

namespace QS.DomainModel.UoW
{
	public interface IUnitOfWorkFactory
	{
		/// <summary>
		/// Создаем Unit of Work без конкретной сущности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		IUnitOfWork Create(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0);
	}
}
