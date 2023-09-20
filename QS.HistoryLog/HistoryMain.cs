using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.HistoryLog.Core;
using QS.Project.DB;

namespace QS.HistoryLog
{
	public static class HistoryMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static void Enable(MySqlConnectionStringBuilder connectionFactory)
		{
			SingleUowEventsTracker.RegisterSingleUowListnerFactory(new TrackerFactory(connectionFactory));
		}
	}

	public interface IFileTrace
	{
		string Name { set; get; }

		uint Size { set; get; }

		bool IsChanged { set; get; }
	}
}

