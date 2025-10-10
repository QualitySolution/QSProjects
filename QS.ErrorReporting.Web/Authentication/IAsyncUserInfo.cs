using System.Threading.Tasks;
using Grpc.Core;

namespace QS.ErrorReporting.Web.Authentication {
	public interface IAsyncUserInfo {
		Task<string> GetNameAsync(ServerCallContext context);
	}
}
