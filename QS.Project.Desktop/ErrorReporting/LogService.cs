using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NLog.Targets;

namespace QS.ErrorReporting
{
	public class LogService : ILogService
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public string GetLog(int? rowCount = null)
		{
			string logFileName = GetLogFilePath();
			if(String.IsNullOrWhiteSpace(logFileName)) {
				return "";
			}
			try {
				if(!rowCount.HasValue) {
					return WriteSafeReadAllText(logFileName);
				}
				
				var allLines = WriteSafeReadAllLines(logFileName);
				return String.Join("\n", allLines.Skip(Math.Max(0, allLines.Count - rowCount.Value)));
			} catch(Exception ex) {
				logger.Error(ex, "Не получилось прочитать лог");
				return "";
			}
		}

		private static string GetLogFilePath()
		{
			var fileTarget = LogManager.Configuration.AllTargets.FirstOrDefault(t => t is FileTarget) as FileTarget;
			return fileTarget == null ? string.Empty : fileTarget.FileName.Render(new LogEventInfo { Level = LogLevel.Debug });
		}
		
		//Так как стандартный метод ReadAllLines не позволяет читать, открытый на запись файл лога.
		public List<string> WriteSafeReadAllLines(String path)
		{
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fileStream))
			{
				List<string> file = new List<string>();
				while (!sr.EndOfStream)
				{
					file.Add(sr.ReadLine());
				}

				return file;
			}
		}
		
		//Так как стандартный метод ReadAllText не позволяет читать, открытый на запись файл лога.
		public string WriteSafeReadAllText(String path)
		{
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fileStream)) {
				return sr.ReadToEnd();
			}
		}
	}
}
