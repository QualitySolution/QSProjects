using System.Threading.Tasks;
using Grpc.Core;

namespace QS.ErrorReporting {
	public interface IAsyncUserInfo {
		Task<string> GetNameAsync(ServerCallContext context);
	}
}
