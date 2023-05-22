using System;
namespace QS.Project.DB {
	public interface IDataBaseInfo
	{
		/// <summary>
		/// Возвращает реальное имя базы данных на сервере.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Возвращает <see langword="true"/> если подключены к демонстрационной базе данных.
		/// </summary>
		bool IsDemo { get; }

		/// <summary>
		/// Возвращает GUID базы
		/// </summary>
		Guid? BaseGuid { get; }

		/// <summary>
		/// Возвращает версию схемы базы
		/// </summary>
		Version Version { get; }
	}
}
