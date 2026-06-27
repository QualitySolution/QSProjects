using System;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;

namespace QS.DbManagement {
	public class ScriptDbFillStrategy : IDbFillStrategy {
		private readonly IDbScriptsConfiguration scripts;

		public ScriptDbFillStrategy(IDbScriptsConfiguration scripts) {
			this.scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
		}

		public IDbCreatorModel CreateFiller(DbFillResources resources) {
			return new MySqlDbCreateModel(
				resources.ConnectionString,
				scripts.MakeCreationScript(),
				resources.Progress,
				resources.Interaction,
				resources.CancellationToken) { FillBaseGuid = false };
		}
	}
}
