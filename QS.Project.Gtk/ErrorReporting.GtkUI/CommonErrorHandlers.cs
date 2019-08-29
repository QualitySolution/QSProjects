using System;
using MySql.Data.MySqlClient;
using QS.Dialog.GtkUI;
using QS.Project.Domain;
using QS.Project.VersionControl;

namespace QS.ErrorReporting.GtkUI
{
	public static class CommonErrorHandlers
	{
		public static bool MySqlExceptionIncorrectStringValue(Exception exception, IApplicationInfo application, UserBase user)
		{
			var mysqlEx = ExceptionHelper.FineExceptionTypeInInner<MySqlException>(exception);
			if(mysqlEx != null && mysqlEx.Number == 1366) {
				MessageDialogHelper.RunErrorDialog("При сохранении в базу данных произошла ошибка «Incorrect string value», " +
					"обычно это означает что вы вставили в поле диалога какие-то символы не поддерживаемые текущей кодировкой поля таблицы. " +
					"Например вы вставили один из символов Эмодзи, при этом кодировка полей в таблицах у вас трех-байтовая utf8, " +
					"для того чтобы сохранять такие символы в MariaDB\\MySQL базу данных, вам необходимо преобразовать все таблицы в " +
					"четырех-байтовую кодировку utf8mb4. Кодировка utf8mb4 используется по умолчанию в новых версиях MariaDB.");
				return true;
			}
			return false;
		}
	}
}
