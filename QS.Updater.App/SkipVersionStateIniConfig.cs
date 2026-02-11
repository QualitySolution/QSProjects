using System;
using QS.Configuration;
using QS.Project.Versioning;
using QS.Utilities.Text;

namespace QS.Updater.App
{
	public class SkipVersionStateIniConfig : ISkipVersionState
	{
		private readonly IChangeableConfiguration configuration;
		private readonly IApplicationInfo applicationInfo;

		public SkipVersionStateIniConfig(IChangeableConfiguration configuration, IApplicationInfo applicationInfo) {
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
		}

		public bool IsSkippedVersion(Version version) {
			var skip = configuration["AppUpdater:SkipVersion"];
			if(!String.IsNullOrEmpty(skip) && Version.TryParse(skip, out Version skipVersion))
				return version.VersionToShortString() == skipVersion.VersionToShortString() 
				       && (applicationInfo.Version.Major >= version.Major && applicationInfo.Version.Minor >= version.Minor);
			return false;
		}

		public void SaveSkipVersion(Version version) {
			configuration["AppUpdater:SkipVersion"] = version.VersionToShortString();
		}
	}
}
