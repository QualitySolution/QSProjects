using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using NLog;

namespace QS.Utilities.Debug
{
	public class PerformanceHelper {
		protected readonly Logger logger;
		List<TimePoint> pointsList = new List<TimePoint>();
		List<TimePoint> currentPointsList;
		List<TimePoint> currentGroupLevels = new List<TimePoint>();

		/// <summary>
		/// Создает новый помощник замера производительности.
		/// </summary>
		/// <param name="nameFirstInterval">Название первого интервала. По умолчанию 'Старт'.</param>
		/// <param name="logger">Логер, если указан при добавлении каждой точки название будет записываться в лог.</param>
		public PerformanceHelper(string nameFirstInterval = "Старт", NLog.Logger logger = null) {
			this.logger = logger;
			currentPointsList = pointsList;
			CheckPoint(nameFirstInterval);
		}

		/// <summary>
		/// Начинаем новый интервал.
		/// </summary>
		/// <param name="name">Название интервала времени</param>
		public virtual void CheckPoint(string name = null) {
			var point = new TimePoint(name ?? currentPointsList.Count.ToString());
			currentPointsList.Add(point);
			logger?.Debug("Time point: " + point.Name);
		}

		/// <summary>
		/// Начинаем новую группу.
		/// </summary>
		/// <param name="name">Название группы</param>
		public virtual void StartGroup(string name) {
			var group = new TimePoint(name);
			group.InternalPoints = new List<TimePoint>();
			currentPointsList.Add(group);
			currentGroupLevels.Add(group);
			currentPointsList = group.InternalPoints;
			logger?.Debug("Start time group: " + group.Name);
		}

		public virtual void EndGroup() {
			CheckPoint("Конец группы");
			var group = currentGroupLevels.Last();
			currentGroupLevels.Remove(group);
			if(currentGroupLevels.Count > 0)
				currentPointsList = currentGroupLevels.Last().InternalPoints;
			else
				currentPointsList = pointsList;
			logger?.Debug("End time group: " + group.Name);
		}

		public void CheckPoint(NLog.Logger logger, string name = null) {
			CheckPoint(name);
			TimePoint previousPoint;
			if(currentPointsList.Count > 1)
				previousPoint = currentPointsList[currentPointsList.Count - 2];
			else
				previousPoint = currentGroupLevels.Last();
			logger.Debug("Замер производительности [{0}] +{1} секунд.", currentPointsList.Last().Name,
						TimePoint.GetElapsed(previousPoint, currentPointsList.Last()).TotalSeconds);
		}

		public void PrintAllPoints(NLog.Logger logger) {
			logger.Debug(PrepareAllPointsToPrint());
		}

		public void PrintAllPoints<T>(ILogger<T> logger) {

			logger.LogDebug(PrepareAllPointsToPrint());
		}

		private string PrepareAllPointsToPrint() {
			TimePoint startPoint = currentPointsList.First();
			TimePoint lastPoint = startPoint;

			var sb = new StringBuilder("Результаты замера производительности");
			sb.Append($"\nНачало в {lastPoint.Time:hh:mm:ss}");

			foreach(var point in currentPointsList.Skip(1)) {
				sb.Append(lastPoint.GetText(0, point));
				lastPoint = point;
			}

			sb.Append($"\nИтого {TimePoint.GetElapsed(startPoint, lastPoint).TotalSeconds} секунд.");

			return sb.ToString();
		}
		
		public TimeSpan TotalTime => TimePoint.GetElapsed(currentPointsList.First(), currentPointsList.Last());

		internal class TimePoint{
			internal string Name { get; }
			/// <summary>
			/// Время создания точки. Используется только для отображения;
			/// интервалы рассчитываются по монотонному <see cref="Stopwatch"/>.
			/// </summary>
			internal DateTime Time { get; }

			internal long Timestamp { get; }

			internal List<TimePoint> InternalPoints;

			internal TimePoint(string name)
			{
				Name = name;
				Time = DateTime.Now;
				Timestamp = Stopwatch.GetTimestamp();
			}

			internal string GetText(int level, TimePoint endPoint)
			{
				var levelstext = new string(' ', level * 2);
				var text = $"\n{levelstext}[{Name}] +{GetElapsed(this, endPoint).TotalSeconds:N6} секунд.";
				if(InternalPoints != null) {
					var lastPoint = InternalPoints.First();
					foreach(var point in InternalPoints.Skip(1)) {
						text += lastPoint.GetText(level + 1, point);
						lastPoint = point;
					}
				}
				return text;
			}

			internal static TimeSpan GetElapsed(TimePoint startPoint, TimePoint endPoint)
			{
				return TimeSpan.FromSeconds(
					(double)(endPoint.Timestamp - startPoint.Timestamp) / Stopwatch.Frequency);
			}
		}

		#region Статическая часть

		public static PerformanceHelper Main;

		public static PerformanceHelper StartMeasurement(string nameFirstPoint = null)
		{
			Main = new PerformanceHelper(nameFirstPoint);
			return Main;
		}

		public static void AddTimePoint(string name = null)
		{
			Main.CheckPoint(name);
		}

		public static void AddTimePoint(NLog.Logger logger, string name = null)
		{
			Main.CheckPoint(logger, name);
		}

		public static void StartPointsGroup(string name)
		{
			Main.StartGroup(name);
		}

		public static void EndPointsGroup()
		{
			Main.EndGroup();
		}

		#endregion
	}
}
