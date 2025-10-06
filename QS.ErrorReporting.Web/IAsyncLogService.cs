using System.Threading.Tasks;

namespace QS.ErrorReporting.Web
{
	public interface IAsyncLogService
	{
		Task<string> GetLogAsync(uint? rowCount = null);
	}
}
