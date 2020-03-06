using System;
namespace QSSupportLib
{
	public interface IErrorReportingSettings
	{
		/// <summary>
		/// Отправлять сообщение об ошибке автоматически
		/// </summary>
		bool SendAutomatically { get; }

		/// <summary>
		/// Дополнительная информация в сообщение об ошибке
		/// </summary>
		string MessageInfo { get; }

		/// <summary>
		/// Ограничение на кол-во строчек лога в сообщении об ошибке
		/// </summary>
		int? LogRowCount { get;}

		/// <summary>
		/// Отправлять сообщение об ошибке автоматически
		/// </summary>
		bool SendErrorRequestEmail { get; }
	}

	public class DefaultErrorReportingSettings : IErrorReportingSettings
	{
		public bool SendAutomatically => false;

		public int? LogRowCount => null;

		public bool SendErrorRequestEmail => true;

		public string MessageInfo => String.Empty;
	}
}
