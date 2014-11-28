using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gtk;

namespace QSValidation
{
	public class QSValidator<T> where T : class
	{
		List<ValidationResult> Results;
		public bool IsValid;

		public QSValidator ()
		{
		}

		public QSValidator (T entity)
		{
			Validate (entity);
		}

		public bool Validate(T entity)
		{
			Results = new List<ValidationResult>();
			var vc = new ValidationContext(entity, null, null);

			if(entity is IValidatableObject)
				Results.AddRange ((entity as IValidatableObject).Validate (vc));

			if (Results.Count > 0) {
				IsValid = false;
				Validator.TryValidateObject (entity, vc, Results, true);
			}
			else
				IsValid = Validator.TryValidateObject(entity, vc, Results, true);

			return IsValid;
		}

		ResultsListDlg CreateMessagesDlg()
		{
			return new ResultsListDlg (Results);
		}

		/// <summary>
		/// Вызываем диалог с сообщениями если они есть.
		/// </summary>
		/// <returns><c>true</c> если есть ошибки, иначе <c>false</c>.</returns>
		/// <param name="parent">родительское окно для модального диалога.</param>
		public bool RunDlgIfNotValid(Window parent)
		{
			if (IsValid)
				return false;

			var dlg = new ResultsListDlg (Results);
			dlg.Parent = parent;
			dlg.Show ();
			dlg.Run ();
			dlg.Destroy ();
			return true;
		}
	}
}

