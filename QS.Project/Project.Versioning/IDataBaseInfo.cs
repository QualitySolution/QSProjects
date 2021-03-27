using System;
namespace QS.Project.Versioning
{
	public interface IDataBaseInfo
	{
		/// <summary>
		/// Возвращает реальное имя базы данных на сервере.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; }

		/// <summary>
		/// Возвращаяет <see langword="true"/> если подключены к демонстрационной базе данных.
		/// </summary>
		bool IsDemo { get; }
	}
}
