using System;
using System.Collections.Generic;
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
			DateTime lastTime;
			if(currentPointsList.Count > 1)
				lastTime = currentPointsList[currentPointsList.Count - 2].Time;
			else
				lastTime = currentGroupLevels.Last().Time;
			logger.Debug("Замер производительности [{0}] +{1} секунд.", currentPointsList.Last().Name,
						(currentPointsList.Last().Time - lastTime).TotalSeconds);
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
			sb.AppendLine($"Начало в {lastPoint.Time:hh:mm:ss}");

			foreach(var point in currentPointsList.Skip(1)) {
				sb.Append(lastPoint.GetText(0, point.Time));
				lastPoint = point;
			}

			sb.AppendLine($"Итого {(lastPoint.Time - startPoint.Time).TotalSeconds} секунд.");

			return sb.ToString();
		}
		
		public TimeSpan TotalTime => currentPointsList.Last().Time - currentPointsList.First().Time;

		public class TimePoint{
			public string Name { get; set;}
			public DateTime Time { get; set;}
			
			public List<TimePoint> InternalPoints;

			public TimePoint(string name) : this(name, DateTime.Now) 
			{}

			public TimePoint(string name, DateTime time)
			{
				Name = name;
				Time = time;
			}

			public string GetText(int level, DateTime endTime)
			{
				var levelstext =  new string(' ', level * 2);
				var text = $"\n{levelstext}[{Name}] +{(endTime - Time).TotalSeconds:N6} секунд.";
				if(InternalPoints != null) {
					var lastPoint = InternalPoints.First();
					foreach(var point in InternalPoints.Skip(1)) {
						text += lastPoint.GetText(level + 1, point.Time);
						lastPoint = point;
					}
				}
				return text;
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

