using System.Threading.Tasks;

namespace QS.ErrorReporting
{
	public interface IAsyncLogService
	{
		Task<string> GetLogAsync(uint? rowCount = null);
	}
}
