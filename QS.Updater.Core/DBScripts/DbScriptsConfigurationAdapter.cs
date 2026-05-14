using System;
using QS.DBScripts.Models;
using QS.Updater.DB;

namespace QS.DBScripts
{
	public class DbScriptsConfigurationAdapter : IDbScriptsConfiguration
	{
		private readonly CreationScript creationScript;
		private readonly UpdateConfiguration updateConfiguration;

		public DbScriptsConfigurationAdapter(CreationScript creationScript, UpdateConfiguration updateConfiguration = null)
		{
			this.creationScript = creationScript ?? throw new ArgumentNullException(nameof(creationScript));
			this.updateConfiguration = updateConfiguration;
		}

		public bool HasCreationScript() => creationScript != null;

		public CreationScript MakeCreationScript() => creationScript;

		public UpdateConfiguration MakeUpdateConfiguration() => updateConfiguration ?? new UpdateConfiguration();
	}
}
