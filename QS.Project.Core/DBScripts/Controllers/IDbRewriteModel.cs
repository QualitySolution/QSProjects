namespace QS.DBScripts.Controllers {
	/// <summary>
	/// Перезапись существующей базы: сохраняет данные, которые должны пережить перезапись,
	/// пересоздаёт содержимое базы переданной моделью наполнения и возвращает сохранённое обратно
	/// </summary>
	public interface IDbRewriteModel {
		/// <returns>false - перезапись прервана (ошибка или несовпадение версий)</returns>
		bool RunRewrite(IDbCreatorModel creationModel, string dbName, string dbTitle);
	}
}
