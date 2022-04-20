using System.Collections.Generic;
using Newtonsoft.Json;

namespace QS.Osrm
{
    public class RouteResponse
	{
		[JsonProperty("routes")]
		public List<Route> Routes { get; set; }

		[JsonProperty("waypoints")]
		public List<Waypoint> Waypoints { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("code")]
		public string Code { get; set; }

		public string StatusMessageRus
		{
			get
			{
				switch(Code)
				{
					case "InvalidQuery":
						return Message.Replace("Query string malformed close to position", "Ошибка в строке запроса, около позиции");
					default:
						return Message;
				}
			}
		}
	}
}
