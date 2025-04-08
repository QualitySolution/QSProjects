using System;

namespace QS.Permissions
{
	/// <summary>
	/// Интерфейс для получения прав доступа к сущностям и предустановленным правам текущего пользователя.
	/// </summary>
	public interface ICurrentPermissionService
	{
		/// <summary>
		/// Проверить права доступа к сущности(документу)
		/// </summary>
		/// <param name="entityType">Тип объекта к которому запрашиваются права.</param>
		/// <param name="documentDate">Опционально. Дата проведения документа или другой сущности в учете. Используется ограничения редактирования по времени, например при использовании даты запрета редактирования документов.</param>
		/// <returns></returns>
		IPermissionResult ValidateEntityPermission(Type entityType, DateTime? documentDate = null);
		bool ValidatePresetPermission(string permissionName);
	}
}
