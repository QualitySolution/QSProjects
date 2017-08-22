using System;
using System.Collections.Generic;
using RestSharp.Deserializers;

namespace QSOsm.Osrm
{
	public class RouteResponse
	{
		[DeserializeAs(Name = "routes")]
		public List<Route> Routes { get; set; }

		[DeserializeAs(Name = "message")]
		public string Message { get; set; }

		[DeserializeAs(Name = "code")]
		public string Code { get; set; }

		[DeserializeAs(Name = "route_geometry")]
		public string RouteGeometry { get; set; }

		public string StatusMessageRus{
			get{
				switch (Code) {
					case "InvalidQuery":
						return Message.Replace("Query string malformed close to position", "Ошибка в строке запроса, около позиции");
				default:
					return Message;
				}
			}
		}
	}
}