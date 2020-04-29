using System;
using RestSharp.Deserializers;

namespace QS.Osm.Osrm
{
	public class Route
	{
		/// <summary>
		/// Общая длина в метрах (целое число)
		/// </summary>
		[DeserializeAs(Name = "distance")]
		public int TotalDistance { get; set; }

		/// <summary>
		/// Общее время в секундах (целое число)
		/// </summary>
		[DeserializeAs(Name = "duration")]
		public int TotalTimeSeconds { get; set; }

		/// <summary>
		/// Геометрия маршрута в polyline
		/// </summary>
		[DeserializeAs(Name = "geometry")]
		public string RouteGeometry { get; set; }

		//[DeserializeAs(Name = "start_point")]
		//public string StartPoint { get; set; } // название начальной улицы (строка)

		//[DeserializeAs(Name = "end_point")]
		//public string EndPoint { get; set; } // название конечной улицы (строка)

		/// <summary>
		/// Общая длина в километрах
		/// </summary>
		public decimal TotalDistanceKm{
			get{
				return (decimal)TotalDistance / 1000;
			}
		}

		/// <summary>
		/// Общее время
		/// </summary>
		public TimeSpan TotalTime{
			get{
				return  TimeSpan.FromSeconds(TotalTimeSeconds);
			}
		}

	}
}

