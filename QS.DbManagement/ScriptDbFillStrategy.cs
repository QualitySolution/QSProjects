using System;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;

namespace QS.DbManagement {
	public class ScriptDbFillStrategy : IDbFillStrategy {
		private readonly IDbScriptsConfiguration scripts;
		private readonly IDbCreatorInteraction interaction;

		public ScriptDbFillStrategy(IDbScriptsConfiguration scripts, IDbCreatorInteraction interaction) {
			this.scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
		}

		public IDbCreatorModel CreateFiller(DbFillResources resources) {
			return new MySqlDbCreateModel(
				resources.ConnectionString,
				scripts.MakeCreationScript(),
				resources.Progress,
				interaction,
				resources.CancellationToken) { FillBaseGuid = false };
		}
	}
}
