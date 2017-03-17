using System;
using RestSharp.Deserializers;

namespace QSOsm.Spuntik
{
	public class RouteSummary
	{
		[DeserializeAs(Name = "total_distance")]
		public int TotalDistance { get; set; } //общая длина в метрах (целое число)

		[DeserializeAs(Name = "total_time")]
		public int TotalTimeSeconds { get; set; } //общее время в секундах (целое число)

		[DeserializeAs(Name = "start_point")]
		public string StartPoint { get; set; } // название начальной улицы (строка)

		[DeserializeAs(Name = "end_point")]
		public string EndPoint { get; set; } // название конечной улицы (строка)

		public decimal TotalDistanceKm{
			get{
				return (decimal)TotalDistance / 1000;
			}
		}

		public TimeSpan TotalTime{
			get{
				return new TimeSpan(0, 0, TotalTimeSeconds);
			}
		}

	}
}

