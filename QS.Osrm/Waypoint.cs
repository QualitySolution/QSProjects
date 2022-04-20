using Newtonsoft.Json;

namespace QS.Osrm
{
    public class Waypoint
	{
		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
