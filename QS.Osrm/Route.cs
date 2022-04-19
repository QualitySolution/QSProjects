using System;
using Newtonsoft.Json;

namespace QS.Osrm
{
	public class Route
	{
		/// <summary>
		/// Общая длина в метрах (целое число)
		/// </summary>
		public int TotalDistance => (int)TotalDistanceDouble;

		/// <summary>
		/// Общее время в секундах (целое число)
		/// </summary>
		public int TotalTimeSeconds => (int)TotalTimeSecondsDouble;

		/// <summary>
		/// Общая длина в метрах
		/// </summary>
		[JsonProperty("distance")]
		public double TotalDistanceDouble { get; set; }

		/// <summary>
		/// Общее время в секундах
		/// </summary>
		[JsonProperty("duration")]
		public double TotalTimeSecondsDouble { get; set; }

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
				return  TimeSpan.FromSeconds((double)TotalTimeSeconds);
			}
		}

	}
}

