using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Configuration {
	public static class ConfigTransition {

		public static List<Dictionary<string, string>> FromOldIniConfig(string filename) {
			var config = new IniFileConfiguration(filename);
			return FromOldIniConfig(config);
		}

		public static List<Dictionary<string, string>> FromOldIniConfig(IChangeableConfiguration config) {
			var result = new List<Dictionary<string, string>>();

			// bullshit but Workwear did the same
			for(int i = 0; i < 100; i++) {
				var section = "Login" + (i >= 0 ? i.ToString() : String.Empty);
				if(config[$"{section}:ConnectionName"] != null) {
					var parameters = new Dictionary<string, string> {
						{ "ConnectionTitle", config[$"{section}:ConnectionName"] },
						{ "Title", config[$"{section}:Type"] == "0" ? "MariaDB" : "QS Cloud" },
						{ "User", config[$"{section}:UserLogin"] }
					};

					if(parameters["Title"] == "QS Cloud")
						parameters.Add("Логин", config[$"{section}:Account"]);

					//TODO: Add correct mapping for specific MariaDB/QS Cloud/etc. parameters

					result.Add(parameters);
				}
			}
			if(config["Default:ConnectionName"] != null) // default connection which is already selected after starting
				result.First(c => c["ConnectionTitle"] == config["Default:ConnectionName"]).Add("Last", "true");

			return result;
		}
	}
}
