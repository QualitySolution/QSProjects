using System;
using MySql.Data.MySqlClient;
using QS.Dialog;
using QS.Project.Domain;
using QS.Project.Versioning;

namespace QS.ErrorReporting
{
	public static class CommonErrorHandlers
	{
		#region MySQL

		public static bool MySqlException1055OnlyFullGroupBy(Exception exception, IApplicationInfo application, UserBase user, IInteractiveMessage interactiveMessage)
		{
			var mysqlEx = ExceptionHelper.FineExceptionTypeInInner<MySqlException>(exception);
			if(mysqlEx != null && mysqlEx.Number == 1055) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "На сервере MariaDB\\MySQL включен режим 'only_full_group_by', " +
					"для нормальной работы программы нужно удалить это значение из опции sql_mode. Обычно по умолчанию этот режим " +
					"отключен, проверьте свой конфигурационный файл my.cnf");
				return true;
			}
			return false;
		}

		public static bool MySqlException1366IncorrectStringValue(Exception exception, IApplicationInfo application, UserBase user, IInteractiveMessage interactiveMessage)
		{
			var mysqlEx = ExceptionHelper.FineExceptionTypeInInner<MySqlException>(exception);
			if(mysqlEx != null && mysqlEx.Number == 1366) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "При сохранении в базу данных произошла ошибка «Incorrect string value», " +
					"обычно это означает что вы вставили в поле диалога какие-то символы не поддерживаемые текущей кодировкой поля таблицы. " +
					"Например вы вставили один из символов Эмодзи, при этом кодировка полей в таблицах у вас трех-байтовая utf8, " +
					"для того чтобы сохранять такие символы в MariaDB\\MySQL базу данных, вам необходимо преобразовать все таблицы в " +
					"четырех-байтовую кодировку utf8mb4. Кодировка utf8mb4 используется по умолчанию в новых версиях MariaDB.");
				return true;
			}
			return false;
		}

		#endregion

		public static bool NHibernateFlushAfterException (Exception exception, IApplicationInfo application, UserBase user, IInteractiveMessage interactiveMessage)
		{
			var nhEx = ExceptionHelper.FineExceptionTypeInInner<NHibernate.AssertionFailure>(exception);
			if(nhEx != null && nhEx.Message.Contains("don't flush the Session after an exception occurs")) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "В этом диалоге ранее произошла ошибка, в следстивии ее программа не может " +
					"сохранить данные. Закройте этот диалог и продолжайте работу в новом.");
				return true;
			}
			return false;
		}
	}
}
