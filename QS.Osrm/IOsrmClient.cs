using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Osrm {
	public interface IOsrmClient {
		RouteResponse GetRoute(List<PointOnEarth> routePOIs, bool alt = false, GeometryOverview geometry = GeometryOverview.False, bool excludeToll = false);
		Task<RouteResponse> GetRouteAsync(List<PointOnEarth> routePOIs, CancellationToken cancellationToken, bool alt = false, GeometryOverview geometry = GeometryOverview.False, bool excludeToll = false);
	}
}
