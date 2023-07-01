using System;
using QS.Configuration;
using QS.Utilities.Text;

namespace QS.Updater.App
{
	public class SkipVersionStateIniConfig : ISkipVersionState
	{
		private readonly IChangeableConfiguration configuration;

		public SkipVersionStateIniConfig(IChangeableConfiguration configuration) {
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public bool IsSkippedVersion(Version version) {
			var skip = configuration["AppUpdater:SkipVersion"];
			if(!String.IsNullOrEmpty(skip) && Version.TryParse(skip, out Version skipVersion))
				return version.VersionToShortString() == skipVersion.VersionToShortString();
			return false;
		}

		public void SaveSkipVersion(Version version) {
			configuration["AppUpdater:SkipVersion"] = version.VersionToShortString();
		}
	}
}
