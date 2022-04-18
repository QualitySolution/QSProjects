using System;
using Newtonsoft.Json;

namespace QS.Osrm
{
	public class Route
	{
		/// <summary>
		/// Общая длина в метрах (целое число)
		/// </summary>
		[JsonProperty("distance")]
		public int TotalDistance { get; set; }

		/// <summary>
		/// Общее время в секундах (целое число)
		/// </summary>
		[JsonProperty("duration")]
		public int TotalTimeSeconds { get; set; }

		/// <summary>
		/// Геометрия маршрута в polyline
		/// </summary>
		[JsonProperty("geometry")]
		public string RouteGeometry { get; set; }

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

