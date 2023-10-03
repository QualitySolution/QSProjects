using System;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Project.DB;
using QS.Utilities.Text;

namespace QS.ErrorReporting.Handlers {
	public class NHibernateStaleObjectStateException : IErrorHandler {
		private readonly IInteractiveMessage interactive;

		public NHibernateStaleObjectStateException(IInteractiveMessage interactive) {
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
		}

		public bool Take(Exception exception) {
			var staleObjectStateException = ExceptionHelper.FindExceptionTypeInInner<NHibernate.StaleObjectStateException>(exception);
			if(staleObjectStateException != null) {
				var type = OrmConfig.FindMappingByFullClassName(staleObjectStateException.EntityName).MappedClass;
				var objectName = DomainHelper.GetSubjectNames(type);

				string message;

				switch(objectName?.Gender) {
					case GrammaticalGender.Feminine:
						message = "<b>{0}</b> c ИД:<b>{1}</b> была изменена другим пользователем.";
						break;
					case GrammaticalGender.Neuter:
						message = "<b>{0}</b> c ИД:<b>{1}</b> было изменено другим пользователем.";
						break;
					case GrammaticalGender.Masculine:
					default:
						message = "<b>{0}</b> c ИД:<b>{1}</b> был изменен другим пользователем.";
						break;
				}
				message = String.Format(message + "\nВаши изменения не могут быть сохранены без потери чужих. \nПереоткройте вкладку.", 
					TitleHelper.StringToTitleCase(objectName?.Nominative ?? type.Name), staleObjectStateException.Identifier);

				interactive.ShowMessage(ImportanceLevel.Warning, message);
				return true;
			}
			return false;
		}
	}
}
