using System;
using System.Collections.Generic;

namespace QSUpdater.DB
{
	public static class DBCreator
	{
		public static readonly List<CreationScript> Scripts = new List<CreationScript>();

		public static void AddBaseScript(Version version, string name, string scriptResource)
		{
			Scripts.Add(new CreationScript
			{
				Version = version,
				Name = name,
				Resource = scriptResource
			});
		}
	}

	public class CreationScript
	{
		public string Name;
		public Version Version;

		public String Resource;
	}
}

