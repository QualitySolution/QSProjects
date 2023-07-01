using System.Threading.Tasks;

namespace QS.ErrorReporting
{
	public interface IAsyncErrorReportSender
	{
		/// <summary>
		/// Отправляет отчет об ошибке. Обратите внимания, при заполнении отчета, все поля в том числе и текстовые не могут быть null.
		/// Их следует заполнять пустыми строками или не заполнять вообще, они все имеют правильные значения по умолчанию.
		/// Особенно это важно при переходе со старого интерфейса.
		/// </summary>
		Task<bool> SubmitErrorReportAsync (SubmitErrorRequest report);
	}
}
