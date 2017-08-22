using System;
using RestSharp.Deserializers;

namespace QSOsm.Osrm
{
	public class Route
	{
		[DeserializeAs(Name = "distance")]
		public int TotalDistance { get; set; } //общая длина в метрах (целое число)

		[DeserializeAs(Name = "duration")]
		public int TotalTimeSeconds { get; set; } //общее время в секундах (целое число)

		[DeserializeAs(Name = "geometry")]
		public string RouteGeometry { get; set; }

		//[DeserializeAs(Name = "start_point")]
		//public string StartPoint { get; set; } // название начальной улицы (строка)

		//[DeserializeAs(Name = "end_point")]
		//public string EndPoint { get; set; } // название конечной улицы (строка)

		public decimal TotalDistanceKm{
			get{
				return (decimal)TotalDistance / 1000;
			}
		}

		public TimeSpan TotalTime{
			get{
				return  TimeSpan.FromSeconds(TotalTimeSeconds);
			}
		}

	}
}

