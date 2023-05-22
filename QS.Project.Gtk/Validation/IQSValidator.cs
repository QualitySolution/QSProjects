using Gtk;

namespace QS.Validation {
	public interface IQSValidator<T>
		where T : class {
		bool RunDlgIfNotValid();
		bool RunDlgIfNotValid(Window parent);
		bool Validate(T entity);
	}
}
