using System;
namespace QS.ErrorReporting
{
	public interface ILogService
	{
		string GetLog(int? rowCount = null);
	}
}
