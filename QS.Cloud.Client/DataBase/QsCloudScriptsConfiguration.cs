using QS.DBScripts;
using QS.DBScripts.Models;
using QS.Updater.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QS.Cloud.Client.DataBase {
	public class QsCloudScriptsConfiguration : IDbScriptsConfiguration {
		private string ResourceName = "QS.Cloud.Client.Scripts.new_empty.sql";
		public bool HasCreationScript() {
			return Assembly.GetAssembly(typeof(QsCloudScriptsConfiguration))
				.GetManifestResourceNames()
				.Contains(ResourceName);
		}

		public CreationScript MakeCreationScript() {
			return new CreationScript(
				Assembly.GetAssembly(typeof(QsCloudScriptsConfiguration)),
				ResourceName,
				new Version(1, 7)
			);
		}

		public UpdateConfiguration MakeUpdateConfiguration() {
			var configuration = new UpdateConfiguration();

			configuration.AddUpdate(
				new Version(1, 7),
				new Version(1, 7, 1),
				"QS.Cloud.Client.Scripts.1.7.sql");

			return configuration;
		}
	}
}
