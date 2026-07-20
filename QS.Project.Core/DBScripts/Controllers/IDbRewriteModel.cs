using System.Data;

namespace QS.DBScripts.Controllers {
	/// <summary>
	/// Что из существующей базы должно пережить перезапись и как это вернуть после наполнения
	/// </summary>
	public interface IDbRewriteModel {
		/// <summary>
		/// Прочитать сохраняемые данные, вызывается до удаления объектов схемы
		/// </summary>
		/// <returns>true - есть что вернуть после наполнения</returns>
		bool Backup(IDbCommand cmd);

		/// <summary>
		/// Вернуть сохранённые данные в базу
		/// </summary>
		void Restore(IDbCommand cmd);
	}
}
