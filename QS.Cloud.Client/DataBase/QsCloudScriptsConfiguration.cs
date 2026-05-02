using QS.DBScripts;
using QS.DBScripts.Models;
using QS.Updater.DB;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QS.Cloud.Client.DataBase {
	public class QsCloudScriptsConfiguration : IDbScriptsConfiguration {
		public CreationScript MakeCreationScript() {
			return new CreationScript(
				Assembly.GetAssembly(typeof(QsCloudScriptsConfiguration)),
				"QS.Cloud.Client.Scripts.new_empty.sql",
				new Version(1, 7)
			);
		}

		public UpdateConfiguration MakeUpdateConfiguration() {
			var configuration = new UpdateConfiguration();

			configuration.AddUpdate(
				new Version(1, 0),
				new Version(1, 0, 1),
				"QS.Cloud.Client.Scripts.1.7.sql");

			return configuration;
		}
	}
}
