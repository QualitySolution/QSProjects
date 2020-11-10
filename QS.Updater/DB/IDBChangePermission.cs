using System;
namespace QS.Updater.DB
{
	/// <summary>
	/// Возможно временный интерфейс пока используется тупо для проброски информации о правах.
	/// </summary>
	public interface IDBChangePermission
	{
		bool HasPermission { get; }
	}
}
