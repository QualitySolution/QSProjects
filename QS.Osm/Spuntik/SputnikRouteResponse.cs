using RestSharp.Deserializers;

namespace QS.Osm.Spuntik
{
	public class SputnikRouteResponse
	{
		[DeserializeAs(Name = "route_summary")]
		public RouteSummary RouteSummary { get; set; }

		[DeserializeAs(Name = "status_message")]
		public string StatusMessage { get; set; }

		[DeserializeAs(Name = "status")]
		public int Status { get; set; }

		[DeserializeAs(Name = "route_geometry")]
		public string RouteGeometry { get; set; }

		public string StatusMessageRus{
			get{
				switch (Status) {
					case 207:
					return "Не удалось найти маршрут между точками";
				default:
					return StatusMessage;
				}
			}
		}
	}
}