namespace QS.ErrorReporting
{
	public interface IErrorReportSender
	{
		/// <summary>
		/// Отправляет отчет об ошибке. Обратите внимания, при заполнении отчета, все поля в том числе и текстовые не могут быть null.
		/// Их следует заполнять пустыми строками или не заполнять вообще, они все имеют правильные значения по умолчанию.
		/// Особенно это важно при переходе со старого интерфейса.
		/// </summary>
		bool SubmitErrorReport (SubmitErrorRequest report);
	}
}
