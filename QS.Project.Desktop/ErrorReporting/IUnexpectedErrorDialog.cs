using System;

namespace QS.ErrorReporting {
	/// <summary>Показывает отчёт без участия навигации приложения.</summary>
	public interface IUnexpectedErrorDialog {
		void Show(Exception exception, string title);
	}
}
