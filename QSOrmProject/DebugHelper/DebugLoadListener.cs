using System;
using NHibernate.Event;

namespace QSOrmProject
{
	public class DebugLoadListener : ILoadEventListener 
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public void OnLoad(LoadEvent theEvent, LoadType loadType)
		{
			logger.Debug("Load Class {0}", theEvent.EntityClassName);
		}
	}
}

