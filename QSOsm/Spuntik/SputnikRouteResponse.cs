using System;
using RestSharp.Deserializers;

namespace QSOsm.Spuntik
{
	public class SputnikRouteResponse
	{
		[DeserializeAs(Name = "route_summary")]
		public RouteSummary RouteSummary { get; set; }

		[DeserializeAs(Name = "status_message")]
		public string StatusMessage { get; set; }

		[DeserializeAs(Name = "status")]
		public int Status { get; set; }

	}
}