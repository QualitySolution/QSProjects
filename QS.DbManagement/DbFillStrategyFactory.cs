using System;
using QS.DBScripts;
using QS.DBScripts.Controllers;

namespace QS.DbManagement {
	public class DbFillStrategyFactory : IDbFillStrategyFactory {
		private readonly IDbScriptsConfiguration scripts;
		private readonly IDbCreatorInteraction interaction;

		public DbFillStrategyFactory(IDbScriptsConfiguration scripts, IDbCreatorInteraction interaction) {
			this.scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
		}

		public IDbFillStrategy ForScript() => new ScriptDbFillStrategy(scripts, interaction);

		public IDbFillStrategy ForDump(string dumpFilePath) => new DumpDbFillStrategy(dumpFilePath);
	}
}
