﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace QS.Utilities.Debug
{
	public class PerformanceHelper
	{
		private readonly Logger logger;
		List<TimePoint> pointsList = new List<TimePoint>();
		List<TimePoint> currentPointsList;
		List<TimePoint> currentGroupLevels = new List<TimePoint>();

		/// <summary>
		/// Создает новый помощник замера производительности.
		/// </summary>
		/// <param name="nameFirstPoint">Название стартовой точки. По умолчанию 'Старт'.</param>
		/// <param name="logger">Логер, если указан при добавлении каждой точки название будет записываться в лог.</param>
		public PerformanceHelper(string nameFirstPoint = "Старт", NLog.Logger logger = null)
		{
			this.logger = logger;
			currentPointsList = pointsList;
			CheckPoint(nameFirstPoint);
		}

		public void CheckPoint(string name = null) {
			var point = new TimePoint(name ?? currentPointsList.Count.ToString());
			currentPointsList.Add(point);
			logger?.Debug("Time point: " + point.Name);
		}

		public void StartGroup(string name)
		{
			var group = new TimePoint()
			{
				Name = name,
				GroupStartTime = DateTime.Now,
				InternalPoints = new List<TimePoint>()
			};
			currentPointsList.Add(group);
			currentGroupLevels.Add(group);
			currentPointsList = group.InternalPoints;
			logger?.Debug("Start time group: " + group.Name);
		}

		public void EndGroup()
		{
			var group = currentGroupLevels.Last();
			group.Time = DateTime.Now;
			currentGroupLevels.Remove(group);
			if (currentGroupLevels.Count > 0)
				currentPointsList = currentGroupLevels.Last().InternalPoints;
			else
				currentPointsList = pointsList;
			logger?.Debug("End time group: " + group.Name);
		}

		public void CheckPoint(NLog.Logger logger, string name = null)
		{
			CheckPoint(name);
			DateTime lastTime;
			if (currentPointsList.Count > 1)
				lastTime = currentPointsList[currentPointsList.Count - 2].Time;
			else
				lastTime = currentGroupLevels.Last().GroupStartTime.Value;
			logger.Debug("Замер производительности [{0}] +{1} секунд.", currentPointsList.Last().Name, 
						(currentPointsList.Last().Time - lastTime).TotalSeconds);
		}

		public void PrintAllPoints(NLog.Logger logger)
		{
			string text = "Результаты замера производительности";
			DateTime lastTime = new DateTime();
			foreach(var point in currentPointsList)
			{
				if (lastTime == default(DateTime))
					text += $"\nНачало в {point.Time:hh:mm:ss}";
				else
				{
					text += point.GetText(0, lastTime);
				}
				lastTime = point.Time;
			}
			text += $"\nИтого {(lastTime - currentPointsList.First().Time).TotalSeconds} секунд.";
			logger.Debug(text);
		}
		
		public TimeSpan TotalTime => currentPointsList.Last().Time - currentPointsList.First().Time;

		public class TimePoint{
			public string Name { get; set;}
			public DateTime Time { get; set;}

			public DateTime? GroupStartTime { get; set;}
			public List<TimePoint> InternalPoints;

			public TimePoint()
			{}

			public TimePoint(string name) : this(name, DateTime.Now) 
			{}

			public TimePoint(string name, DateTime time)
			{
				Name = name;
				Time = time;
			}

			public string GetText(int level, DateTime lastTime)
			{
				var levelstext =  new string(' ', level * 2);
				if(GroupStartTime == null)
				{
					return $"\n{levelstext}[{Name}] +{(Time - lastTime).TotalSeconds:N6} секунд.";
				}
				else
				{

					var text = $"\n{levelstext}({Name} - {(Time - GroupStartTime.Value).TotalSeconds:N6} сек.) +{(Time - lastTime).TotalSeconds:N6} секунд.";
					var internalLast = GroupStartTime.Value;
					foreach(var point in InternalPoints)
					{
						text += point.GetText(level + 1, internalLast);
						internalLast = point.Time;
					}
					return text;
				}
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

