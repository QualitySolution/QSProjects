using Gtk;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation {
	public class QSValidator<T> : IQSValidator<T> where T : class {
		public List<ValidationResult> Results;
		public bool IsValid;

		public IDictionary<object, object> ContextItems = null;

		public QSValidator() {
		}

		public QSValidator(T entity, IDictionary<object, object> contextItems = null) {
			ContextItems = contextItems;
			Validate(entity);
		}

		public bool Validate(T entity) {
			Results = new List<ValidationResult>();
			var vc = new ValidationContext(entity, null, ContextItems);

			IsValid = Validator.TryValidateObject(entity, vc, Results, true);

			return IsValid;
		}

		ResultsListDlg CreateMessagesDlg() {
			return new ResultsListDlg(Results);
		}

		/// <summary>
		/// Вызываем диалог с сообщениями если они есть.
		/// </summary>
		/// <returns><c>true</c> если есть ошибки, иначе <c>false</c>.</returns>
		/// <param name="parent">родительское окно для модального диалога.</param>
		public bool RunDlgIfNotValid(Window parent) {
			if(IsValid)
				return false;

			var dlg = new ResultsListDlg(Results);
			dlg.Parent = parent;
			dlg.Show();
			dlg.Run();
			dlg.Destroy();
			return true;
		}

		public bool RunDlgIfNotValid() {
			if(IsValid)
				return false;

			var dlg = new ResultsListDlg(Results);
			dlg.Parent = null;
			dlg.Modal = true;
			dlg.Show();
			dlg.Run();
			dlg.Destroy();
			return true;
		}
	}
}

